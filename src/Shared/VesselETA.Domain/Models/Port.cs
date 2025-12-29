namespace VesselETA.Domain.Models;

public record Port
{
    public string PortCode { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public bool IsTidal { get; init; }
    public List<TideWindow> TideWindows { get; init; } = new();
    public string Country { get; init; } = "UK";
    public double[][] BoundingBox { get; init; } = new double[0][];
}

public record TideWindow
{
    public TimeOnly StartTime { get; init; }
    public TimeOnly EndTime { get; init; }
    public string Description { get; init; } = string.Empty;
}