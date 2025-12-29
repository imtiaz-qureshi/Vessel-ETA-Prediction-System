using System.Net.WebSockets;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VesselETA.Domain.Models;
using VesselETA.Infrastructure.Kafka;

namespace VesselETA.AisIngestion;

public class RealAisIngestionWorker : BackgroundService
{
    private readonly ILogger<RealAisIngestionWorker> _logger;
    private readonly IKafkaProducer<VesselPosition> _kafkaProducer;
    private readonly IConfiguration _configuration;

    public RealAisIngestionWorker(
        ILogger<RealAisIngestionWorker> logger,
        IKafkaProducer<VesselPosition> kafkaProducer,
        IConfiguration configuration)
    {
        _logger = logger;
        _kafkaProducer = kafkaProducer;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Real AIS Ingestion Worker starting at: {Time}", DateTimeOffset.Now);

        var apiKey = _configuration["AisStream:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogError("AIS Stream API key not configured. Please set AisStream:ApiKey in configuration.");
            return;
        }

        // Configure bounding boxes for areas of interest
        var boundingBoxes = GetBoundingBoxes();
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConnectAndStreamAisDataAsync(apiKey, boundingBoxes, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AIS stream connection. Retrying in 30 seconds...");
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }

    private async Task ConnectAndStreamAisDataAsync(string apiKey, double[][][] boundingBoxes, CancellationToken stoppingToken)
    {
        using var ws = new ClientWebSocket();
        
        try
        {
            _logger.LogInformation("Connecting to AIS Stream...");
            await ws.ConnectAsync(new Uri("wss://stream.aisstream.io/v0/stream"), stoppingToken);
            _logger.LogInformation("Connected to AIS Stream. WebSocket State: {State}", ws.State);
            
            // Send subscription message
            var subscriptionMessage = new
            {
                APIKey = apiKey,
                BoundingBoxes = boundingBoxes
            };
            
            string json = JsonConvert.SerializeObject(subscriptionMessage);
            _logger.LogInformation("Sending subscription: {Json}", json);
            
            await ws.SendAsync(
                new ArraySegment<byte>(Encoding.UTF8.GetBytes(json)),
                WebSocketMessageType.Text,
                true,
                stoppingToken);

            _logger.LogInformation("Subscription sent successfully. Starting to receive data...");

            // Receive and process messages
            byte[] buffer = new byte[8192]; // Increased buffer size for larger messages
            
            while (ws.State == WebSocketState.Open && !stoppingToken.IsCancellationRequested)
            {
                var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), stoppingToken);
                
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    _logger.LogInformation("WebSocket connection closed by server");
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, stoppingToken);
                    break;
                }
                else if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    _logger.LogDebug("Received AIS message: {Message}", message.Length > 200 ? message.Substring(0, 200) + "..." : message);
                    await ProcessAisMessageAsync(message, stoppingToken);
                }
                else
                {
                    _logger.LogDebug("Received message of type {MessageType} with {Count} bytes", result.MessageType, result.Count);
                }
            }
            _logger.LogInformation("AIS stream connection closed. Final WebSocket State: {State}", ws.State);;
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("AIS stream connection cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AIS stream connection");
            throw;
        }
        finally
        {
            if (ws.State == WebSocketState.Open)
            {
                try
                {
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Service stopping", CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error closing WebSocket connection");
                }
            }
        }
    }

    private async Task ProcessAisMessageAsync(string message, CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogDebug("Processing AIS message of length {Length}", message.Length);
            
            var aisMessage = JsonConvert.DeserializeObject<AisStreamMessage>(message);
            
            if (aisMessage?.MetaData?.MMSI == null)
            {
                _logger.LogDebug("Received message without MMSI, skipping. MessageType: {MessageType}", aisMessage?.MessageType ?? "Unknown");
                return;
            }

            _logger.LogDebug("Processing {MessageType} for MMSI {MMSI}", aisMessage.MessageType, aisMessage.MetaData.MMSI);

            var vesselPosition = MapToVesselPosition(aisMessage);
            
            if (vesselPosition != null)
            {
                await _kafkaProducer.PublishAsync(
                    KafkaTopics.RawAisPositions,
                    vesselPosition.Mmsi,
                    vesselPosition,
                    stoppingToken);

                _logger.LogInformation("Published AIS position for vessel {Mmsi} ({Name}) at {Lat}, {Lon}",
                    vesselPosition.Mmsi, vesselPosition.VesselName, vesselPosition.Latitude, vesselPosition.Longitude);
            }
            else
            {
                _logger.LogDebug("Skipped message type {MessageType} for MMSI {MMSI}", aisMessage.MessageType, aisMessage.MetaData.MMSI);
            }
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse AIS message: {Message}", message.Length > 500 ? message.Substring(0, 500) + "..." : message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing AIS message");
        }
    }

    private VesselPosition? MapToVesselPosition(AisStreamMessage aisMessage)
    {
        try
        {
            // Handle different message types
            if (aisMessage.MessageType == "PositionReport" && aisMessage.Message?.PositionReport != null)
            {
                var posReport = aisMessage.Message.PositionReport;
                return new VesselPosition
                {
                    Mmsi = aisMessage.MetaData.MMSI.ToString(),
                    VesselName = aisMessage.MetaData.ShipName?.Trim() ?? string.Empty,
                    Latitude = posReport.Latitude,
                    Longitude = posReport.Longitude,
                    SpeedKnots = posReport.Sog,
                    Course = posReport.Cog,
                    TimestampUtc = DateTime.TryParse(aisMessage.MetaData.time_utc, out var dt) ? dt : DateTime.UtcNow
                };
            }
            else if (aisMessage.MessageType == "StandardClassBPositionReport" && aisMessage.Message?.StandardClassBPositionReport != null)
            {
                var posReport = aisMessage.Message.StandardClassBPositionReport;
                return new VesselPosition
                {
                    Mmsi = aisMessage.MetaData.MMSI.ToString(),
                    VesselName = aisMessage.MetaData.ShipName?.Trim() ?? string.Empty,
                    Latitude = posReport.Latitude,
                    Longitude = posReport.Longitude,
                    SpeedKnots = posReport.Sog,
                    Course = posReport.Cog,
                    TimestampUtc = DateTime.TryParse(aisMessage.MetaData.time_utc, out var dt) ? dt : DateTime.UtcNow
                };
            }
            
            return null; // Skip other message types for now
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error mapping AIS message to vessel position");
            return null;
        }
    }

    private double[][][] GetBoundingBoxes()
    {
        // Default to UK waters and major shipping lanes
        var boundingBoxes = new List<double[][]>();
        
        // Add more bounding boxes from configuration if available
        var configBoxes = _configuration.GetSection("AisStream:BoundingBoxes").Get<double[][][]>();
        if (configBoxes != null && configBoxes.Length > 0)
        {
            boundingBoxes.AddRange(configBoxes);
            _logger.LogInformation("Loaded {Count} bounding boxes from configuration", configBoxes.Length);
            
            // Log each bounding box for debugging
            for (int i = 0; i < configBoxes.Length; i++)
            {
                var box = configBoxes[i];
                if (box.Length == 2 && box[0].Length == 2 && box[1].Length == 2)
                {
                    _logger.LogInformation("Bounding Box {Index}: SW({Lat1},{Lon1}) to NE({Lat2},{Lon2})", 
                        i + 1, box[0][0], box[0][1], box[1][0], box[1][1]);
                }
            }
        }
        else
        {
            // Fallback to the same bounding box as the working sample
            _logger.LogWarning("No bounding boxes configured, using default sample area");
            double[][] boundingBox = new double[][] { 
                new double[] { 51.9686, 1.2650 }, 
                new double[] { 51.9332, 1.3500 } 
            };
            boundingBoxes.Add(boundingBox);
        }
        
        _logger.LogInformation("Using {Count} bounding boxes for AIS data", boundingBoxes.Count);
        return boundingBoxes.ToArray();
    }
}