using VesselETA.Domain.Models;
using VesselETA.Infrastructure.Services;

namespace VesselETA.EtaEngine.Services;

public interface IEtaCalculationService
{
    Task<EtaPrediction?> CalculateEtaAsync(VesselPosition position, CancellationToken cancellationToken = default);
}

public class EtaCalculationService : IEtaCalculationService
{
    private readonly IDistanceCalculator _distanceCalculator;
    private readonly IPortRepository _portRepository;
    private readonly IWeatherService _weatherService;
    private readonly IDelayRiskAssessment _delayRiskAssessment;
    private readonly ILogger<EtaCalculationService> _logger;

    public EtaCalculationService(
        IDistanceCalculator distanceCalculator,
        IPortRepository portRepository,
        IWeatherService weatherService,
        IDelayRiskAssessment delayRiskAssessment,
        ILogger<EtaCalculationService> logger)
    {
        _distanceCalculator = distanceCalculator;
        _portRepository = portRepository;
        _weatherService = weatherService;
        _delayRiskAssessment = delayRiskAssessment;
        _logger = logger;
    }

    public async Task<EtaPrediction?> CalculateEtaAsync(VesselPosition position, CancellationToken cancellationToken = default)
    {
        try
        {
            // Find nearest port
            var nearestPort = await _portRepository.GetNearestPortAsync(position.Latitude, position.Longitude);
            if (nearestPort == null)
            {
                _logger.LogWarning("No port found for vessel {Mmsi} at position {Lat}, {Lon}", 
                    position.Mmsi, position.Latitude, position.Longitude);
                return null;
            }

            // Calculate distance
            var distanceNm = _distanceCalculator.CalculateDistanceNauticalMiles(
                position.Latitude, position.Longitude,
                nearestPort.Latitude, nearestPort.Longitude);

            // Get weather data
            var weatherData = await _weatherService.GetWeatherDataAsync(
                position.Latitude, position.Longitude, cancellationToken);

            // Calculate base ETA
            var effectiveSpeed = CalculateEffectiveSpeed(position.SpeedKnots, weatherData);
            var baseEtaHours = distanceNm / Math.Max(effectiveSpeed, 1.0); // Avoid division by zero
            var baseEta = DateTime.UtcNow.AddHours(baseEtaHours);

            // Apply tidal constraints
            var adjustedEta = ApplyTidalConstraints(baseEta, nearestPort);

            // Create weather impact
            var weatherImpact = new WeatherImpact
            {
                SpeedReductionFactor = 1.0 - weatherData.SeverityFactor * 0.3, // Max 30% reduction
                Conditions = weatherData.Conditions,
                WindSpeedKnots = weatherData.WindSpeedKnots,
                WaveHeightMeters = weatherData.WaveHeightMeters
            };

            // Create prediction
            var prediction = new EtaPrediction
            {
                Mmsi = position.Mmsi,
                PortCode = nearestPort.PortCode,
                EstimatedArrivalUtc = adjustedEta,
                DelayRisk = DelayRisk.Low, // Will be calculated by risk assessment
                DistanceNauticalMiles = distanceNm,
                AverageSpeedKnots = effectiveSpeed,
                WeatherImpact = weatherImpact,
                TidalConstraint = adjustedEta != baseEta,
                PredictionTimestampUtc = DateTime.UtcNow
            };

            // Assess delay risk
            prediction = prediction with 
            { 
                DelayRisk = await _delayRiskAssessment.AssessDelayRiskAsync(prediction, weatherData)
            };

            _logger.LogDebug("Calculated ETA for vessel {Mmsi} to port {Port}: {Eta} (Distance: {Distance:F1}nm, Speed: {Speed:F1}kn)",
                position.Mmsi, nearestPort.PortCode, adjustedEta, distanceNm, effectiveSpeed);

            return prediction;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating ETA for vessel {Mmsi}", position.Mmsi);
            return null;
        }
    }

    private static double CalculateEffectiveSpeed(double baseSpeedKnots, WeatherData weatherData)
    {
        // Apply weather-based speed reduction
        var speedReductionFactor = 1.0 - (weatherData.SeverityFactor * 0.3); // Max 30% reduction
        return Math.Max(baseSpeedKnots * speedReductionFactor, 1.0); // Minimum 1 knot
    }

    private static DateTime ApplyTidalConstraints(DateTime baseEta, Port port)
    {
        if (!port.IsTidal || !port.TideWindows.Any())
            return baseEta;

        var etaTime = TimeOnly.FromDateTime(baseEta);
        
        // Find next available tide window
        foreach (var window in port.TideWindows.OrderBy(w => w.StartTime))
        {
            if (IsTimeInWindow(etaTime, window))
            {
                return baseEta; // Already in a valid window
            }

            if (etaTime < window.StartTime)
            {
                // Wait for this window to open
                var waitTime = window.StartTime.ToTimeSpan() - etaTime.ToTimeSpan();
                return baseEta.Add(waitTime);
            }
        }

        // If we're past all windows today, wait for the first window tomorrow
        var firstWindow = port.TideWindows.OrderBy(w => w.StartTime).First();
        var tomorrow = baseEta.Date.AddDays(1);
        var nextWindowStart = tomorrow.Add(firstWindow.StartTime.ToTimeSpan());
        
        return nextWindowStart;
    }

    private static bool IsTimeInWindow(TimeOnly time, TideWindow window)
    {
        if (window.StartTime <= window.EndTime)
        {
            // Same day window
            return time >= window.StartTime && time <= window.EndTime;
        }
        else
        {
            // Overnight window
            return time >= window.StartTime || time <= window.EndTime;
        }
    }
}