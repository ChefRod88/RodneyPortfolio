using Microsoft.AspNetCore.SignalR;

namespace ChurchWebsite.Hubs;

/// <summary>
/// Real-time location hub — Uber-style two-way WebSocket channel.
/// Clients stream GPS coordinates; server confirms with a timestamp so the
/// client can display round-trip WebSocket latency.
/// </summary>
public class LocationHub : Hub
{
    /// <summary>
    /// Called by the client to report a GPS position update.
    /// Returns a confirmation with the server-side UTC timestamp so the client
    /// can calculate round-trip latency.
    /// </summary>
    public async Task SendLocation(double latitude, double longitude, double accuracy)
    {
        await Clients.Caller.SendAsync("LocationConfirmed", new
        {
            latitude,
            longitude,
            accuracy,
            serverTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        });
    }
}
