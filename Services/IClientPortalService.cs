// ══════════════════════════════════════════════════════════════
// REPLACE your existing IClientPortalService.cs with this.
// Adds: GetAllAccountsAsync, DeleteAccountAsync, UpdateAccountAsync
// ══════════════════════════════════════════════════════════════
using RodneyPortfolio.Models;

namespace RodneyPortfolio.Services;

public interface IClientPortalService
{
    // Accounts — existing
    Task<ClientAccount?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<ClientAccount?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
    Task SaveAccountAsync(ClientAccount account, CancellationToken ct = default);

    // Accounts — new admin methods
    Task<List<ClientAccount>> GetAllAccountsAsync(CancellationToken ct = default);
    Task<bool> DeleteAccountAsync(string id, CancellationToken ct = default);
    Task UpdateAccountAsync(ClientAccount account, CancellationToken ct = default);

    // OTP — existing
    Task<string> GenerateOtpAsync(string email, string purpose, CancellationToken ct = default);
    Task<bool> ValidateOtpAsync(string email, string code, string purpose, CancellationToken ct = default);

    // Sessions — existing
    Task<ClientSession> CreateSessionAsync(string clientId, string email, CancellationToken ct = default);
    Task<ClientSession?> GetSessionAsync(string sessionId, CancellationToken ct = default);
    Task InvalidateSessionAsync(string sessionId, CancellationToken ct = default);
}
