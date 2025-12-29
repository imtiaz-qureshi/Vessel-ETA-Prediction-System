using Microsoft.AspNetCore.SignalR;
using VesselETA.ApiGateway.Hubs;
using VesselETA.Domain.Models;
using VesselETA.Infrastructure.Kafka;

namespace VesselETA.ApiGateway.Services;

public interface IEtaStreamService
{
    Task StartStreamingAsync(CancellationToken cancellationToken);
}

public class EtaStreamService : IEtaStreamService
{
    private readonly IKafkaConsumer<EtaPrediction> _kafkaConsumer;
    private readonly IHubContext<EtaHub> _hubContext;
    private readonly IVesselService _vesselService;
    private readonly ILogger<EtaStreamService> _logger;

    public EtaStreamService(
        IKafkaConsumer<EtaPrediction> kafkaConsumer,
        IHubContext<EtaHub> hubContext,
        IVesselService vesselService,
        ILogger<EtaStreamService> logger)
    {
        _kafkaConsumer = kafkaConsumer;
        _hubContext = hubContext;
        _vesselService = vesselService;
        _logger = logger;
    }

    public async Task StartStreamingAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting ETA streaming service");

        await _kafkaConsumer.StartConsumingAsync(
            KafkaTopics.EtaPredictions,
            ProcessEtaPredictionAsync,
            cancellationToken);
    }

    private async Task ProcessEtaPredictionAsync(EtaPrediction prediction)
    {
        try
        {
            // Update vessel service
            _vesselService.UpdateEtaPrediction(prediction);

            // Broadcast to SignalR clients
            await _hubContext.Clients.All.SendAsync("EtaUpdate", prediction);

            // Send to specific groups if needed
            await _hubContext.Clients.Group($"vessel-{prediction.Mmsi}")
                .SendAsync("VesselEtaUpdate", prediction);

            await _hubContext.Clients.Group($"port-{prediction.PortCode}")
                .SendAsync("PortEtaUpdate", prediction);

            _logger.LogDebug("Broadcasted ETA update for vessel {Mmsi} to SignalR clients",
                prediction.Mmsi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing ETA prediction for vessel {Mmsi}",
                prediction.Mmsi);
        }
    }
}

public class EtaStreamBackgroundService : BackgroundService
{
    private readonly IEtaStreamService _etaStreamService;
    private readonly ILogger<EtaStreamBackgroundService> _logger;

    public EtaStreamBackgroundService(
        IEtaStreamService etaStreamService,
        ILogger<EtaStreamBackgroundService> logger)
    {
        _etaStreamService = etaStreamService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ETA Stream Background Service starting");

        // Add a delay to allow the main application to start first
        await Task.Delay(2000, stoppingToken);

        try
        {
            await _etaStreamService.StartStreamingAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ETA Stream Background Service - continuing without Kafka streaming");
            // Don't throw - let the application continue without Kafka streaming
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}