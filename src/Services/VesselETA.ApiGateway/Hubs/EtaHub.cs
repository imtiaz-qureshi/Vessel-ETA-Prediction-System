using Microsoft.AspNetCore.SignalR;

namespace VesselETA.ApiGateway.Hubs;

public class EtaHub : Hub
{
    private readonly ILogger<EtaHub> _logger;

    public EtaHub(ILogger<EtaHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SubscribeToVessel(string mmsi)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"vessel-{mmsi}");
        _logger.LogInformation("Client {ConnectionId} subscribed to vessel {Mmsi}",
            Context.ConnectionId, mmsi);
    }

    public async Task UnsubscribeFromVessel(string mmsi)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"vessel-{mmsi}");
        _logger.LogInformation("Client {ConnectionId} unsubscribed from vessel {Mmsi}",
            Context.ConnectionId, mmsi);
    }

    public async Task SubscribeToPort(string portCode)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"port-{portCode}");
        _logger.LogInformation("Client {ConnectionId} subscribed to port {PortCode}",
            Context.ConnectionId, portCode);
    }

    public async Task UnsubscribeFromPort(string portCode)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"port-{portCode}");
        _logger.LogInformation("Client {ConnectionId} unsubscribed from port {PortCode}",
            Context.ConnectionId, portCode);
    }
}