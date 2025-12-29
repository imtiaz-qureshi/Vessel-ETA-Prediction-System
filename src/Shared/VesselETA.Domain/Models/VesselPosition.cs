namespace VesselETA.Domain.Models;

public record VesselPosition
{
    public string Mmsi { get; init; } = string.Empty;
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public double SpeedKnots { get; init; }
    public double Course { get; init; }
    public DateTime TimestampUtc { get; init; }
    public string? VesselName { get; init; }
    public string? CallSign { get; init; }
}