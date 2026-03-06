using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RodneyPortfolio.Services;

namespace RodneyPortfolio.Pages.Admin;

public class AdminLoginModel : PageModel
{
    private readonly string _configuredPinHash;
    private readonly ILogger<AdminLoginModel> _logger;

    // Simple in-process rate limiter: key = IP, value = (failCount, firstFailAt)
    private static readonly Dictionary<string, (int Count, DateTime First)> _failedAttempts = new();
    private static readonly object _lock = new();
    private const int MaxAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    public AdminLoginModel(IConfiguration configuration, ILogger<AdminLoginModel> logger)
    {
        _configuredPinHash = configuration["Admin:PinHash"] ?? HashPin("2479");
        _logger = logger;
    }

    [BindProperty] public string Pin { get; set; } = string.Empty;
    public string? ErrorMessage { get; private set; }

    public void OnGet() { }

    public IActionResult OnPostLogin()
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        // Check lockout
        lock (_lock)
        {
            if (_failedAttempts.TryGetValue(ip, out var entry))
            {
                if (entry.Count >= MaxAttempts && DateTime.UtcNow - entry.First < LockoutDuration)
                {
                    _logger.LogWarning("Admin login blocked for IP {IP} — too many failed attempts", ip);
                    ErrorMessage = "Too many failed attempts. Try again in 15 minutes.";
                    return Page();
                }
                // Reset expired lockout
                if (DateTime.UtcNow - entry.First >= LockoutDuration)
                    _failedAttempts.Remove(ip);
            }
        }

        var hash = HashPin(Pin);
        if (hash != _configuredPinHash)
        {
            lock (_lock)
            {
                if (_failedAttempts.TryGetValue(ip, out var entry))
                    _failedAttempts[ip] = (entry.Count + 1, entry.First);
                else
                    _failedAttempts[ip] = (1, DateTime.UtcNow);
            }
            _logger.LogWarning("Failed admin login attempt from IP {IP}", ip);
            ErrorMessage = "Invalid PIN. Access denied.";
            return Page();
        }

        // Success — clear failed attempts
        lock (_lock) { _failedAttempts.Remove(ip); }

        AdminGuard.MarkAuthenticated(HttpContext);
        _logger.LogInformation("Admin authenticated from IP {IP}", ip);
        return RedirectToPage("/Admin/Accounts");
    }

    public IActionResult OnPostLogout()
    {
        AdminGuard.ClearAuthentication(HttpContext);
        return RedirectToPage("/Admin/AdminLogin");
    }

    private static string HashPin(string pin)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(pin.Trim());
        var hash = sha.ComputeHash(bytes);
        return Convert.ToHexString(hash).ToLower();
    }
}
