using RodneyPortfolio.Models;

namespace RodneyPortfolio.Services;

/// <summary>
/// Creates, retrieves, and invalidates authenticated client sessions.
/// </summary>
public interface ISessionService
{
    Task<ClientSession> CreateSessionAsync(string clientId, string email, CancellationToken ct = default);
    Task<ClientSession?> GetSessionAsync(string sessionId, CancellationToken ct = default);
    Task InvalidateSessionAsync(string sessionId, CancellationToken ct = default);
}
