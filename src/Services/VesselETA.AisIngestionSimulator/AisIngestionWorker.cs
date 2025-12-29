using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VesselETA.Domain.Models;
using VesselETA.Infrastructure.Kafka;

namespace VesselETA.AisIngestionSimulator;

public class AisIngestionWorker : BackgroundService
{
    private readonly ILogger<AisIngestionWorker> _logger;
    private readonly IKafkaProducer<VesselPosition> _kafkaProducer;
    private readonly IConfiguration _configuration;

    public AisIngestionWorker(
        ILogger<AisIngestionWorker> logger,
        IKafkaProducer<VesselPosition> kafkaProducer,
        IConfiguration configuration)
    {
        _logger = logger;
        _kafkaProducer = kafkaProducer;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AIS Ingestion Worker starting at: {Time}", DateTimeOffset.Now);

        var dataFilePath = _configuration["AisDataFilePath"] ?? "Data/ais-sample.csv";
        var replayIntervalMs = _configuration.GetValue<int>("ReplayIntervalMs", 1000);

        if (!File.Exists(dataFilePath))
        {
            _logger.LogWarning("AIS data file not found at {Path}. Generating sample data...", dataFilePath);
            await GenerateSampleDataAsync(dataFilePath, stoppingToken);
        }

        await ReplayAisDataAsync(dataFilePath, replayIntervalMs, stoppingToken);
    }

    private async Task ReplayAisDataAsync(string filePath, int intervalMs, CancellationToken stoppingToken)
    {
        try
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null
            };

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, config);

            var records = csv.GetRecords<AisCsvRecord>().ToList();
            _logger.LogInformation("Loaded {Count} AIS records from {Path}", records.Count, filePath);

            var recordIndex = 0;
            while (!stoppingToken.IsCancellationRequested)
            {
                var record = records[recordIndex % records.Count];
                var position = MapToVesselPosition(record);

                await _kafkaProducer.PublishAsync(
                    KafkaTopics.RawAisPositions,
                    position.Mmsi,
                    position,
                    stoppingToken);

                _logger.LogDebug("Published AIS position for vessel {Mmsi} at {Lat}, {Lon}",
                    position.Mmsi, position.Latitude, position.Longitude);

                recordIndex++;
                await Task.Delay(intervalMs, stoppingToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error replaying AIS data");
            throw;
        }
    }

    private async Task GenerateSampleDataAsync(string filePath, CancellationToken stoppingToken)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var sampleData = GenerateSampleAisData();
        
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true
        };

        await using var writer = new StreamWriter(filePath);
        await using var csv = new CsvWriter(writer, config);
        await csv.WriteRecordsAsync(sampleData, stoppingToken);

        _logger.LogInformation("Generated {Count} sample AIS records at {Path}", sampleData.Count, filePath);
    }

    private static List<AisCsvRecord> GenerateSampleAisData()
    {
        // Sample vessels approaching UK ports
        var vessels = new[]
        {
            new { Mmsi = "235012345", Name = "CONTAINER_SHIP_1", StartLat = 51.5, StartLon = 0.5, TargetLat = 51.9514, TargetLon = 1.3062, Speed = 18.0 }, // To Felixstowe
            new { Mmsi = "235012346", Name = "BULK_CARRIER_1", StartLat = 53.0, StartLon = -4.0, TargetLat = 53.4668, TargetLon = -3.0338, Speed = 14.0 }, // To Liverpool
            new { Mmsi = "235012347", Name = "TANKER_1", StartLat = 51.3, StartLon = 0.8, TargetLat = 51.5074, TargetLon = 0.1278, Speed = 12.0 }, // To London Gateway
        };

        var records = new List<AisCsvRecord>();
        var baseTime = DateTime.UtcNow.AddHours(-2);

        foreach (var vessel in vessels)
        {
            for (int i = 0; i < 20; i++)
            {
                var progress = i / 20.0;
                var lat = vessel.StartLat + (vessel.TargetLat - vessel.StartLat) * progress;
                var lon = vessel.StartLon + (vessel.TargetLon - vessel.StartLon) * progress;

                records.Add(new AisCsvRecord
                {
                    Mmsi = vessel.Mmsi,
                    VesselName = vessel.Name,
                    Latitude = lat,
                    Longitude = lon,
                    SpeedKnots = vessel.Speed + Random.Shared.NextDouble() * 2 - 1,
                    Course = CalculateCourse(vessel.StartLat, vessel.StartLon, vessel.TargetLat, vessel.TargetLon),
                    Timestamp = baseTime.AddMinutes(i * 30).ToString("o")
                });
            }
        }

        return records;
    }

    private static double CalculateCourse(double lat1, double lon1, double lat2, double lon2)
    {
        var dLon = (lon2 - lon1) * Math.PI / 180;
        var y = Math.Sin(dLon) * Math.Cos(lat2 * Math.PI / 180);
        var x = Math.Cos(lat1 * Math.PI / 180) * Math.Sin(lat2 * Math.PI / 180) -
                Math.Sin(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) * Math.Cos(dLon);
        var bearing = Math.Atan2(y, x) * 180 / Math.PI;
        return (bearing + 360) % 360;
    }

    private static VesselPosition MapToVesselPosition(AisCsvRecord record)
    {
        return new VesselPosition
        {
            Mmsi = record.Mmsi,
            Latitude = record.Latitude,
            Longitude = record.Longitude,
            SpeedKnots = record.SpeedKnots,
            Course = record.Course,
            TimestampUtc = DateTime.TryParse(record.Timestamp, out var dt) ? dt : DateTime.UtcNow,
            VesselName = record.VesselName
        };
    }

    private class AisCsvRecord
    {
        public string Mmsi { get; set; } = string.Empty;
        public string VesselName { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double SpeedKnots { get; set; }
        public double Course { get; set; }
        public string Timestamp { get; set; } = string.Empty;
    }
}