using VesselETA.Domain.Models;

namespace VesselETA.ApiGateway.Services;

public interface IVesselService
{
    Task<EtaPrediction?> GetLatestEtaAsync(string mmsi);
    Task<List<EtaPrediction>> GetEtaHistoryAsync(string mmsi, int hours);
    Task<List<EtaPrediction>> GetAllVesselsAsync();
    void UpdateEtaPrediction(EtaPrediction prediction);
}

public class VesselService : IVesselService
{
    private readonly Dictionary<string, List<EtaPrediction>> _etaHistory = new();
    private readonly Dictionary<string, EtaPrediction> _latestEtas = new();
    private readonly ILogger<VesselService> _logger;

    public VesselService(ILogger<VesselService> logger)
    {
        _logger = logger;
    }

    public Task<EtaPrediction?> GetLatestEtaAsync(string mmsi)
    {
        _latestEtas.TryGetValue(mmsi, out var eta);
        return Task.FromResult(eta);
    }

    public Task<List<EtaPrediction>> GetEtaHistoryAsync(string mmsi, int hours)
    {
        if (!_etaHistory.TryGetValue(mmsi, out var history))
        {
            return Task.FromResult(new List<EtaPrediction>());
        }

        var cutoff = DateTime.UtcNow.AddHours(-hours);
        var filteredHistory = history
            .Where(eta => eta.PredictionTimestampUtc > cutoff)
            .OrderBy(eta => eta.PredictionTimestampUtc)
            .ToList();

        return Task.FromResult(filteredHistory);
    }

    public Task<List<EtaPrediction>> GetAllVesselsAsync()
    {
        var allVessels = _latestEtas.Values.ToList();
        return Task.FromResult(allVessels);
    }

    public void UpdateEtaPrediction(EtaPrediction prediction)
    {
        // Update latest ETA
        _latestEtas[prediction.Mmsi] = prediction;

        // Add to history
        if (!_etaHistory.ContainsKey(prediction.Mmsi))
        {
            _etaHistory[prediction.Mmsi] = new List<EtaPrediction>();
        }

        _etaHistory[prediction.Mmsi].Add(prediction);

        // Clean up old history (keep last 7 days)
        var cutoff = DateTime.UtcNow.AddDays(-7);
        _etaHistory[prediction.Mmsi] = _etaHistory[prediction.Mmsi]
            .Where(eta => eta.PredictionTimestampUtc > cutoff)
            .ToList();

        _logger.LogDebug("Updated ETA for vessel {Mmsi}: {Eta} to port {Port}",
            prediction.Mmsi, prediction.EstimatedArrivalUtc, prediction.PortCode);
    }
}