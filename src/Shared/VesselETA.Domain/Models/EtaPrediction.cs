namespace VesselETA.Domain.Models;

public record EtaPrediction
{
    public string Mmsi { get; init; } = string.Empty;
    public string PortCode { get; init; } = string.Empty;
    public DateTime EstimatedArrivalUtc { get; init; }
    public DelayRisk DelayRisk { get; init; }
    public double DistanceNauticalMiles { get; init; }
    public double AverageSpeedKnots { get; init; }
    public WeatherImpact WeatherImpact { get; init; }
    public bool TidalConstraint { get; init; }
    public DateTime PredictionTimestampUtc { get; init; }
}

public enum DelayRisk
{
    Low,
    Medium,
    High
}

public record WeatherImpact
{
    public double SpeedReductionFactor { get; init; }
    public string Conditions { get; init; } = string.Empty;
    public double WindSpeedKnots { get; init; }
    public double WaveHeightMeters { get; init; }
}