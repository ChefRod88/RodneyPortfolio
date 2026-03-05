using Microsoft.EntityFrameworkCore;
using RodneyPortfolio.Data;
using RodneyPortfolio.Models;

namespace RodneyPortfolio.Services;

public class SqlClientPortalService : IClientPortalService
{
    private readonly AppDbContext _db;
    private readonly ILogger<SqlClientPortalService> _logger;

    public SqlClientPortalService(AppDbContext db, ILogger<SqlClientPortalService> logger)
    {
        _db = db;
        _logger = logger;
    }

    // ── Accounts ──────────────────────────────────────────────
    public async Task<ClientAccount?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await _db.ClientAccounts
            .FirstOrDefaultAsync(a => a.Email.ToLower() == email.ToLower(), ct);

    public async Task<ClientAccount?> GetByIdAsync(string id, CancellationToken ct = default)
        => await _db.ClientAccounts.FindAsync(new object[] { id }, ct);

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
        => await _db.ClientAccounts.AnyAsync(a => a.Email.ToLower() == email.ToLower(), ct);

    public async Task SaveAccountAsync(ClientAccount account, CancellationToken ct = default)
    {
        var exists = await _db.ClientAccounts.AnyAsync(a => a.Id == account.Id, ct);
        if (exists)
            _db.ClientAccounts.Update(account);
        else
            await _db.ClientAccounts.AddAsync(account, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<List<ClientAccount>> GetAllAccountsAsync(CancellationToken ct = default)
        => await _db.ClientAccounts
            .OrderByDescending(a => a.RegisteredAt)
            .ToListAsync(ct);

    public async Task<bool> DeleteAccountAsync(string id, CancellationToken ct = default)
    {
        var account = await _db.ClientAccounts.FindAsync(new object[] { id }, ct);
        if (account is null) return false;

        // Remove their sessions too
        var sessions = await _db.ClientSessions
            .Where(s => s.ClientId == id).ToListAsync(ct);
        _db.ClientSessions.RemoveRange(sessions);

        _db.ClientAccounts.Remove(account);
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Deleted account {Id}", id);
        return true;
    }

    // ── OTP ───────────────────────────────────────────────────
    public async Task<string> GenerateOtpAsync(string email, string purpose, CancellationToken ct = default)
    {
        // Invalidate existing unused codes
        var existing = await _db.OtpCodes
            .Where(c => c.Email == email && c.Purpose == purpose && !c.Used)
            .ToListAsync(ct);
        foreach (var c in existing) c.Used = true;

        var code = Random.Shared.Next(100000, 1000000).ToString();
        await _db.OtpCodes.AddAsync(new OtpCode
        {
            Email = email,
            Code = code,
            Purpose = purpose,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10)
        }, ct);

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("OTP generated for {Email} [{Purpose}]", email, purpose);
        return code;
    }

    public async Task<bool> ValidateOtpAsync(string email, string code, string purpose, CancellationToken ct = default)
    {
        var otp = await _db.OtpCodes.FirstOrDefaultAsync(c =>
            c.Email.ToLower() == email.ToLower() &&
            c.Code == code &&
            c.Purpose == purpose &&
            !c.Used &&
            c.ExpiresAt > DateTimeOffset.UtcNow, ct);

        if (otp is null) return false;

        otp.Used = true;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    // ── Sessions ──────────────────────────────────────────────
    public async Task<ClientSession> CreateSessionAsync(string clientId, string email, CancellationToken ct = default)
    {
        // Clean up expired sessions
        var expired = await _db.ClientSessions
            .Where(s => s.ExpiresAt < DateTimeOffset.UtcNow)
            .ToListAsync(ct);
        _db.ClientSessions.RemoveRange(expired);

        var session = new ClientSession { ClientId = clientId, Email = email };
        await _db.ClientSessions.AddAsync(session, ct);
        await _db.SaveChangesAsync(ct);
        return session;
    }

    public async Task<ClientSession?> GetSessionAsync(string sessionId, CancellationToken ct = default)
    {
        var session = await _db.ClientSessions.FindAsync(new object[] { sessionId }, ct);
        return session is { Expired: false } ? session : null;
    }

    public async Task InvalidateSessionAsync(string sessionId, CancellationToken ct = default)
    {
        var session = await _db.ClientSessions.FindAsync(new object[] { sessionId }, ct);
        if (session is not null)
        {
            _db.ClientSessions.Remove(session);
            await _db.SaveChangesAsync(ct);
        }
    }
}
