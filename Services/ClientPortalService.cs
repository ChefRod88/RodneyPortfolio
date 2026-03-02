// ══════════════════════════════════════════════════════════════
// REPLACE your existing ClientPortalService.cs with this.
// Adds: GetAllAccountsAsync, DeleteAccountAsync
// ══════════════════════════════════════════════════════════════
using System.Text.Json;
using RodneyPortfolio.Models;

namespace RodneyPortfolio.Services;

public class ClientPortalService : IClientPortalService
{
    private readonly string _dataDir;
    private readonly ILogger<ClientPortalService> _logger;
    private static readonly JsonSerializerOptions _json = new() { WriteIndented = true };

    public ClientPortalService(IWebHostEnvironment env, ILogger<ClientPortalService> logger)
    {
        _dataDir = Path.Combine(env.ContentRootPath, "Data");
        Directory.CreateDirectory(_dataDir);
        _logger = logger;
    }

    private string AccountsPath  => Path.Combine(_dataDir, "ClientAccounts.json");
    private string OtpPath       => Path.Combine(_dataDir, "OtpCodes.json");
    private string SessionsPath  => Path.Combine(_dataDir, "ClientSessions.json");

    private async Task<List<T>> LoadAsync<T>(string path)
    {
        if (!File.Exists(path)) return new();
        var json = await File.ReadAllTextAsync(path);
        return JsonSerializer.Deserialize<List<T>>(json) ?? new();
    }

    private async Task SaveAsync<T>(string path, List<T> items)
        => await File.WriteAllTextAsync(path, JsonSerializer.Serialize(items, _json));

    // ── Accounts ──────────────────────────────────────────────
    public async Task<ClientAccount?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var accounts = await LoadAsync<ClientAccount>(AccountsPath);
        return accounts.FirstOrDefault(a => a.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<ClientAccount?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var accounts = await LoadAsync<ClientAccount>(AccountsPath);
        return accounts.FirstOrDefault(a => a.Id == id);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
        => await GetByEmailAsync(email, ct) is not null;

    public async Task SaveAccountAsync(ClientAccount account, CancellationToken ct = default)
    {
        var accounts = await LoadAsync<ClientAccount>(AccountsPath);
        var idx = accounts.FindIndex(a => a.Id == account.Id);
        if (idx >= 0) accounts[idx] = account; else accounts.Add(account);
        await SaveAsync(AccountsPath, accounts);
    }

    public async Task<List<ClientAccount>> GetAllAccountsAsync(CancellationToken ct = default)
    {
        var accounts = await LoadAsync<ClientAccount>(AccountsPath);
        return accounts.OrderByDescending(a => a.RegisteredAt).ToList();
    }

    public async Task<bool> DeleteAccountAsync(string id, CancellationToken ct = default)
    {
        var accounts = await LoadAsync<ClientAccount>(AccountsPath);
        var removed = accounts.RemoveAll(a => a.Id == id);
        if (removed > 0)
        {
            await SaveAsync(AccountsPath, accounts);
            // Also clean up their sessions
            var sessions = await LoadAsync<ClientSession>(SessionsPath);
            sessions.RemoveAll(s => s.ClientId == id);
            await SaveAsync(SessionsPath, sessions);
            _logger.LogInformation("Deleted account {Id}", id);
            return true;
        }
        return false;
    }

    // ── OTP ───────────────────────────────────────────────────
    public async Task<string> GenerateOtpAsync(string email, string purpose, CancellationToken ct = default)
    {
        var codes = await LoadAsync<OtpCode>(OtpPath);
        foreach (var c in codes.Where(c => c.Email == email && c.Purpose == purpose && !c.Used))
            c.Used = true;
        var code = Random.Shared.Next(100000, 999999).ToString();
        codes.Add(new OtpCode { Email = email, Code = code, Purpose = purpose, ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10) });
        await SaveAsync(OtpPath, codes);
        return code;
    }

    public async Task<bool> ValidateOtpAsync(string email, string code, string purpose, CancellationToken ct = default)
    {
        var codes = await LoadAsync<OtpCode>(OtpPath);
        var otp = codes.FirstOrDefault(c =>
            c.Email.Equals(email, StringComparison.OrdinalIgnoreCase) &&
            c.Code == code && c.Purpose == purpose && !c.Used &&
            c.ExpiresAt > DateTimeOffset.UtcNow);
        if (otp is null) return false;
        otp.Used = true;
        await SaveAsync(OtpPath, codes);
        return true;
    }

    // ── Sessions ──────────────────────────────────────────────
    public async Task<ClientSession> CreateSessionAsync(string clientId, string email, CancellationToken ct = default)
    {
        var sessions = await LoadAsync<ClientSession>(SessionsPath);
        sessions.RemoveAll(s => s.Expired);
        var session = new ClientSession { ClientId = clientId, Email = email };
        sessions.Add(session);
        await SaveAsync(SessionsPath, sessions);
        return session;
    }

    public async Task<ClientSession?> GetSessionAsync(string sessionId, CancellationToken ct = default)
    {
        var sessions = await LoadAsync<ClientSession>(SessionsPath);
        var session = sessions.FirstOrDefault(s => s.Id == sessionId);
        return session is { Expired: false } ? session : null;
    }

    public async Task InvalidateSessionAsync(string sessionId, CancellationToken ct = default)
    {
        var sessions = await LoadAsync<ClientSession>(SessionsPath);
        sessions.RemoveAll(s => s.Id == sessionId);
        await SaveAsync(SessionsPath, sessions);
    }
}
