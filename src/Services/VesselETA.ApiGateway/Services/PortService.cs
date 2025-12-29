using VesselETA.Domain.Models;

namespace VesselETA.ApiGateway.Services;

public interface IPortService
{
    Task<List<Port>> GetAllPortsAsync();
    Task<Port?> GetPortByCodeAsync(string portCode);
    Task<List<EtaPrediction>> GetVesselsAtPortAsync(string portCode);
}

public class PortService : IPortService
{
    private readonly List<Port> _ports;
    private readonly IVesselService _vesselService;

    public PortService(IVesselService vesselService)
    {
        _vesselService = vesselService;
        _ports = InitializePorts();
    }

    public Task<List<Port>> GetAllPortsAsync()
    {
        return Task.FromResult(_ports);
    }

    public Task<Port?> GetPortByCodeAsync(string portCode)
    {
        var port = _ports.FirstOrDefault(p => p.PortCode.Equals(portCode, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(port);
    }

    public async Task<List<EtaPrediction>> GetVesselsAtPortAsync(string portCode)
    {
        var allVessels = await _vesselService.GetAllVesselsAsync();
        return allVessels
            .Where(v => v.PortCode.Equals(portCode, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private static List<Port> InitializePorts()
    {
        return new List<Port>
        {
            new()
            {
                PortCode = "FXT",
                Name = "Felixstowe",
                Latitude = 51.9514,
                Longitude = 1.3062,
                IsTidal = true,
                TideWindows = new List<TideWindow>
                {
                    new() { StartTime = new TimeOnly(8, 43), EndTime = new TimeOnly(14, 43), Description = "Morning High Tide" },
                    new() { StartTime = new TimeOnly(20, 57), EndTime = new TimeOnly(2, 57), Description = "Evening High Tide" }
                },
                BoundingBox = new double[][] { new double[] { 51.9686, 1.2650 }, new double[] { 51.9332, 1.3500 } }
            },
            new()
            {
                PortCode = "LGW",
                Name = "London Gateway",
                Latitude = 51.5074,
                Longitude = 0.1278,
                IsTidal = true,
                TideWindows = new List<TideWindow>
                {
                    new() { StartTime = new TimeOnly(9, 15), EndTime = new TimeOnly(15, 15), Description = "Morning High Tide" },
                    new() { StartTime = new TimeOnly(21, 30), EndTime = new TimeOnly(3, 30), Description = "Evening High Tide" }
                },
                BoundingBox = new double[][] { new double[] { 51.5250, -0.1500 }, new double[] { 51.4800, 0.0250 } }
            },
            new()
            {
                PortCode = "LIV",
                Name = "Liverpool",
                Latitude = 53.4668,
                Longitude = -3.0338,
                IsTidal = true,
                TideWindows = new List<TideWindow>
                {
                    new() { StartTime = new TimeOnly(7, 30), EndTime = new TimeOnly(13, 30), Description = "Morning High Tide" },
                    new() { StartTime = new TimeOnly(19, 45), EndTime = new TimeOnly(1, 45), Description = "Evening High Tide" }
                },
                BoundingBox = new double[][] { new double[] { 53.5000, -3.1000 }, new double[] { 53.4500, -3.0250 } }
            },
            new()
            {
                PortCode = "IMM",
                Name = "Immingham",
                Latitude = 53.6333,
                Longitude = -0.2167,
                IsTidal = true,
                TideWindows = new List<TideWindow>
                {
                    new() { StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(14, 0), Description = "Morning High Tide" },
                    new() { StartTime = new TimeOnly(20, 15), EndTime = new TimeOnly(2, 15), Description = "Evening High Tide" }
                },
                BoundingBox = new double[][] { new double[] { 53.6500, -0.2500 }, new double[] { 53.6000, -0.1500 } }
            }
        };
    }
}