using VesselETA.Domain.Models;

namespace VesselETA.EtaEngine.Services;

public interface IDelayRiskAssessment
{
    Task<DelayRisk> AssessDelayRiskAsync(EtaPrediction prediction, WeatherData weatherData);
}

public class DelayRiskAssessment : IDelayRiskAssessment
{
    private readonly ILogger<DelayRiskAssessment> _logger;
    private readonly Dictionary<string, List<EtaPrediction>> _etaHistory = new();

    public DelayRiskAssessment(ILogger<DelayRiskAssessment> logger)
    {
        _logger = logger;
    }

    public Task<DelayRisk> AssessDelayRiskAsync(EtaPrediction prediction, WeatherData weatherData)
    {
        var riskFactors = new List<RiskFactor>();

        // Weather-based risk
        var weatherRisk = AssessWeatherRisk(weatherData);
        riskFactors.Add(weatherRisk);

        // ETA drift risk (requires historical data)
        var driftRisk = AssessEtaDriftRisk(prediction);
        riskFactors.Add(driftRisk);

        // Distance-based risk
        var distanceRisk = AssessDistanceRisk(prediction);
        riskFactors.Add(distanceRisk);

        // Tidal constraint risk
        var tidalRisk = AssessTidalRisk(prediction);
        riskFactors.Add(tidalRisk);

        // Calculate overall risk
        var overallRisk = CalculateOverallRisk(riskFactors);

        _logger.LogDebug("Risk assessment for vessel {Mmsi}: Weather={Weather}, Drift={Drift}, Distance={Distance}, Tidal={Tidal}, Overall={Overall}",
            prediction.Mmsi, weatherRisk.Level, driftRisk.Level, distanceRisk.Level, tidalRisk.Level, overallRisk);

        // Store prediction for drift analysis
        StoreEtaPrediction(prediction);

        return Task.FromResult(overallRisk);
    }

    private static RiskFactor AssessWeatherRisk(WeatherData weatherData)
    {
        var severity = weatherData.SeverityFactor;
        
        return severity switch
        {
            < 0.3 => new RiskFactor { Level = DelayRisk.Low, Weight = 0.3, Description = "Favorable weather conditions" },
            < 0.6 => new RiskFactor { Level = DelayRisk.Medium, Weight = 0.3, Description = "Moderate weather conditions" },
            _ => new RiskFactor { Level = DelayRisk.High, Weight = 0.3, Description = "Severe weather conditions" }
        };
    }

    private RiskFactor AssessEtaDriftRisk(EtaPrediction prediction)
    {
        if (!_etaHistory.TryGetValue(prediction.Mmsi, out var history) || history.Count < 2)
        {
            return new RiskFactor { Level = DelayRisk.Low, Weight = 0.4, Description = "Insufficient historical data" };
        }

        // Calculate ETA drift over the last hour
        var recentPredictions = history
            .Where(p => p.PredictionTimestampUtc > DateTime.UtcNow.AddHours(-1))
            .OrderBy(p => p.PredictionTimestampUtc)
            .ToList();

        if (recentPredictions.Count < 2)
        {
            return new RiskFactor { Level = DelayRisk.Low, Weight = 0.4, Description = "Stable ETA" };
        }

        var firstEta = recentPredictions.First().EstimatedArrivalUtc;
        var lastEta = recentPredictions.Last().EstimatedArrivalUtc;
        var driftMinutes = Math.Abs((lastEta - firstEta).TotalMinutes);

        return driftMinutes switch
        {
            < 15 => new RiskFactor { Level = DelayRisk.Low, Weight = 0.4, Description = "Minimal ETA drift" },
            < 30 => new RiskFactor { Level = DelayRisk.Medium, Weight = 0.4, Description = "Moderate ETA drift" },
            _ => new RiskFactor { Level = DelayRisk.High, Weight = 0.4, Description = "Significant ETA drift" }
        };
    }

    private static RiskFactor AssessDistanceRisk(EtaPrediction prediction)
    {
        var distance = prediction.DistanceNauticalMiles;
        
        return distance switch
        {
            < 50 => new RiskFactor { Level = DelayRisk.Low, Weight = 0.2, Description = "Close to port" },
            < 200 => new RiskFactor { Level = DelayRisk.Medium, Weight = 0.2, Description = "Moderate distance" },
            _ => new RiskFactor { Level = DelayRisk.High, Weight = 0.2, Description = "Long distance to port" }
        };
    }

    private static RiskFactor AssessTidalRisk(EtaPrediction prediction)
    {
        if (!prediction.TidalConstraint)
        {
            return new RiskFactor { Level = DelayRisk.Low, Weight = 0.1, Description = "No tidal constraints" };
        }

        // If arrival is constrained by tides, there's inherent risk
        var hoursToArrival = (prediction.EstimatedArrivalUtc - DateTime.UtcNow).TotalHours;
        
        return hoursToArrival switch
        {
            < 2 => new RiskFactor { Level = DelayRisk.High, Weight = 0.1, Description = "Tight tidal window" },
            < 6 => new RiskFactor { Level = DelayRisk.Medium, Weight = 0.1, Description = "Moderate tidal constraint" },
            _ => new RiskFactor { Level = DelayRisk.Low, Weight = 0.1, Description = "Flexible tidal window" }
        };
    }

    private static DelayRisk CalculateOverallRisk(List<RiskFactor> riskFactors)
    {
        var weightedScore = riskFactors.Sum(rf => (int)rf.Level * rf.Weight);
        var totalWeight = riskFactors.Sum(rf => rf.Weight);
        var averageRisk = weightedScore / totalWeight;

        return averageRisk switch
        {
            < 0.7 => DelayRisk.Low,
            < 1.3 => DelayRisk.Medium,
            _ => DelayRisk.High
        };
    }

    private void StoreEtaPrediction(EtaPrediction prediction)
    {
        if (!_etaHistory.ContainsKey(prediction.Mmsi))
        {
            _etaHistory[prediction.Mmsi] = new List<EtaPrediction>();
        }

        _etaHistory[prediction.Mmsi].Add(prediction);

        // Keep only last 24 hours of predictions
        var cutoff = DateTime.UtcNow.AddHours(-24);
        _etaHistory[prediction.Mmsi] = _etaHistory[prediction.Mmsi]
            .Where(p => p.PredictionTimestampUtc > cutoff)
            .ToList();
    }

    private class RiskFactor
    {
        public DelayRisk Level { get; set; }
        public double Weight { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}