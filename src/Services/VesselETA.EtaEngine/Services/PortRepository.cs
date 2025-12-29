using VesselETA.Domain.Models;

namespace VesselETA.EtaEngine.Services;

public interface IPortRepository
{
    Task<List<Port>> GetAllPortsAsync();
    Task<Port?> GetPortByCodeAsync(string portCode);
    Task<Port?> GetNearestPortAsync(double latitude, double longitude);
}

public class PortRepository : IPortRepository
{
    private readonly List<Port> _ports;

    public PortRepository()
    {
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

    public Task<Port?> GetNearestPortAsync(double latitude, double longitude)
    {
        var nearestPort = _ports
            .OrderBy(p => CalculateDistance(latitude, longitude, p.Latitude, p.Longitude))
            .FirstOrDefault();
        
        return Task.FromResult(nearestPort);
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
                }
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
                }
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
                }
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
                }
            }
        };
    }

    private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadius = 6371; // km
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return earthRadius * c;
    }
}