namespace VesselETA.Domain.Models;

public record WeatherData
{
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public double WindSpeedKnots { get; init; }
    public double WindDirection { get; init; }
    public double WaveHeightMeters { get; init; }
    public double VisibilityKm { get; init; }
    public string Conditions { get; init; } = string.Empty;
    public DateTime TimestampUtc { get; init; }
    public double SeverityFactor { get; init; } // 0.0 to 1.0, where 1.0 is severe
}