using VesselETA.Domain.Models;
using VesselETA.EtaEngine.Services;
using VesselETA.Infrastructure.Kafka;

namespace VesselETA.EtaEngine;

public class EtaEngineWorker : BackgroundService
{
    private readonly ILogger<EtaEngineWorker> _logger;
    private readonly IKafkaConsumer<VesselPosition> _kafkaConsumer;
    private readonly IKafkaProducer<EtaPrediction> _kafkaProducer;
    private readonly IEtaCalculationService _etaCalculationService;

    public EtaEngineWorker(
        ILogger<EtaEngineWorker> logger,
        IKafkaConsumer<VesselPosition> kafkaConsumer,
        IKafkaProducer<EtaPrediction> kafkaProducer,
        IEtaCalculationService etaCalculationService)
    {
        _logger = logger;
        _kafkaConsumer = kafkaConsumer;
        _kafkaProducer = kafkaProducer;
        _etaCalculationService = etaCalculationService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ETA Engine Worker starting at: {Time}", DateTimeOffset.Now);

        await _kafkaConsumer.StartConsumingAsync(
            KafkaTopics.RawAisPositions,
            ProcessVesselPositionAsync,
            stoppingToken);
    }

    private async Task ProcessVesselPositionAsync(VesselPosition position)
    {
        try
        {
            _logger.LogDebug("Processing vessel position for {Mmsi} at {Lat}, {Lon}",
                position.Mmsi, position.Latitude, position.Longitude);

            var etaPrediction = await _etaCalculationService.CalculateEtaAsync(position);
            
            if (etaPrediction != null)
            {
                await _kafkaProducer.PublishAsync(
                    KafkaTopics.EtaPredictions,
                    etaPrediction.Mmsi,
                    etaPrediction);

                _logger.LogInformation("Published ETA prediction for vessel {Mmsi} to port {Port}: {Eta} (Risk: {Risk})",
                    etaPrediction.Mmsi, etaPrediction.PortCode, etaPrediction.EstimatedArrivalUtc, etaPrediction.DelayRisk);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing vessel position for {Mmsi}", position.Mmsi);
        }
    }
}