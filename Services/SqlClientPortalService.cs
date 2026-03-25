using Microsoft.EntityFrameworkCore;
using RodneyPortfolio.Data;
using RodneyPortfolio.Models;

namespace RodneyPortfolio.Services;

/// <summary>
/// SQL Server-backed implementation of IAccountService, IOtpService, and ISessionService.
/// </summary>
public class SqlClientPortalService : IAccountService, IOtpService, ISessionService
{
    private readonly AppDbContext _db;
    private readonly ILogger<SqlClientPortalService> _logger;

    public SqlClientPortalService(AppDbContext db, ILogger<SqlClientPortalService> logger)
    {
        _db = db;
        _logger = logger;
    }

    // ── IAccountService ───────────────────────────────────────
    public async Task<ClientAccount?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        try
        {
            var normalized = email.Trim().ToLowerInvariant();
            return await _db.ClientAccounts
                .FirstOrDefaultAsync(a => a.Email.ToLower() == normalized, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving account by email {Email}", email);
            throw;
        }
    }

    public async Task<ClientAccount?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        try
        {
            return await _db.ClientAccounts.FindAsync(new object[] { id }, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving account by id {Id}", id);
            throw;
        }
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
    {
        try
        {
            var normalized = email.Trim().ToLowerInvariant();
            return await _db.ClientAccounts.AnyAsync(a => a.Email.ToLower() == normalized, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking email existence for {Email}", email);
            throw;
        }
    }

    public async Task SaveAccountAsync(ClientAccount account, CancellationToken ct = default)
    {
        try
        {
            var exists = await _db.ClientAccounts.AnyAsync(a => a.Id == account.Id, ct);
            if (exists)
                _db.ClientAccounts.Update(account);
            else
                await _db.ClientAccounts.AddAsync(account, ct);
            await _db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving account {Id}", account.Id);
            throw;
        }
    }

    public async Task<List<ClientAccount>> GetAllAccountsAsync(CancellationToken ct = default)
    {
        try
        {
            return await _db.ClientAccounts
                .OrderByDescending(a => a.RegisteredAt)
                .ToListAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all accounts");
            throw;
        }
    }

    public async Task UpdateAccountAsync(ClientAccount account, CancellationToken ct = default)
    {
        try
        {
            _db.ClientAccounts.Update(account);
            await _db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating account {Id}", account.Id);
            throw;
        }
    }

    public async Task<bool> DeleteAccountAsync(string id, CancellationToken ct = default)
    {
        try
        {
            var account = await _db.ClientAccounts.FindAsync(new object[] { id }, ct);
            if (account is null) return false;

            var sessions = await _db.ClientSessions
                .Where(s => s.ClientId == id).ToListAsync(ct);
            _db.ClientSessions.RemoveRange(sessions);

            _db.ClientAccounts.Remove(account);
            await _db.SaveChangesAsync(ct);
            _logger.LogInformation("Deleted account {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting account {Id}", id);
            throw;
        }
    }

    // ── IOtpService ───────────────────────────────────────────
    public async Task<string> GenerateOtpAsync(string email, string purpose, CancellationToken ct = default)
    {
        try
        {
            var existing = await _db.OtpCodes
                .Where(c => c.Email == email && c.Purpose == purpose && !c.Used)
                .ToListAsync(ct);
            foreach (var c in existing) c.Used = true;

            var code = Random.Shared.Next(100000, 1000000).ToString();
            await _db.OtpCodes.AddAsync(new OtpCode
            {
                Email     = email,
                Code      = code,
                Purpose   = purpose,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10)
            }, ct);

            await _db.SaveChangesAsync(ct);
            _logger.LogInformation("OTP generated for {Email} [{Purpose}]", email, purpose);
            return code;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating OTP for {Email} [{Purpose}]", email, purpose);
            throw;
        }
    }

    public async Task<bool> ValidateOtpAsync(string email, string code, string purpose, CancellationToken ct = default)
    {
        try
        {
            var normalized = email.Trim().ToLowerInvariant();
            var otp = await _db.OtpCodes.FirstOrDefaultAsync(c =>
                c.Email.ToLower() == normalized &&
                c.Code == code &&
                c.Purpose == purpose &&
                !c.Used &&
                c.ExpiresAt > DateTimeOffset.UtcNow, ct);

            if (otp is null) return false;

            otp.Used = true;
            await _db.SaveChangesAsync(ct);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating OTP for {Email} [{Purpose}]", email, purpose);
            throw;
        }
    }

    // ── ISessionService ───────────────────────────────────────
    public async Task<ClientSession> CreateSessionAsync(string clientId, string email, CancellationToken ct = default)
    {
        try
        {
            var expired = await _db.ClientSessions
                .Where(s => s.ExpiresAt < DateTimeOffset.UtcNow)
                .ToListAsync(ct);
            _db.ClientSessions.RemoveRange(expired);

            var session = new ClientSession { ClientId = clientId, Email = email };
            await _db.ClientSessions.AddAsync(session, ct);
            await _db.SaveChangesAsync(ct);
            return session;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating session for client {ClientId}", clientId);
            throw;
        }
    }

    public async Task<ClientSession?> GetSessionAsync(string sessionId, CancellationToken ct = default)
    {
        try
        {
            var session = await _db.ClientSessions.FindAsync(new object[] { sessionId }, ct);
            return session is { Expired: false } ? session : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving session {SessionId}", sessionId);
            throw;
        }
    }

    public async Task InvalidateSessionAsync(string sessionId, CancellationToken ct = default)
    {
        try
        {
            var session = await _db.ClientSessions.FindAsync(new object[] { sessionId }, ct);
            if (session is not null)
            {
                _db.ClientSessions.Remove(session);
                await _db.SaveChangesAsync(ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating session {SessionId}", sessionId);
            throw;
        }
    }
}
