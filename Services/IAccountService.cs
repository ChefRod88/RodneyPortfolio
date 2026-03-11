using RodneyPortfolio.Models;

namespace RodneyPortfolio.Services;

/// <summary>
/// Manages client account records: lookup, creation, update, and deletion.
/// </summary>
public interface IAccountService
{
    Task<ClientAccount?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<ClientAccount?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
    Task SaveAccountAsync(ClientAccount account, CancellationToken ct = default);
    Task<List<ClientAccount>> GetAllAccountsAsync(CancellationToken ct = default);
    Task<bool> DeleteAccountAsync(string id, CancellationToken ct = default);
    Task UpdateAccountAsync(ClientAccount account, CancellationToken ct = default);
}
