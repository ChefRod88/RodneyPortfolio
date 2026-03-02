using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RodneyPortfolio.Services;

namespace RodneyPortfolio.Pages.Admin;

public class AdminLoginModel : PageModel
{
    private readonly string _configuredPinHash;

    public AdminLoginModel(IConfiguration configuration)
    {
        // Prefer configured hash; fallback preserves existing local behavior.
        _configuredPinHash = configuration["Admin:PinHash"] ?? HashPin("2479");
    }

    [BindProperty] public string Pin { get; set; } = string.Empty;
    public string? ErrorMessage { get; private set; }

    public void OnGet() { }

    public IActionResult OnPostLogin()
    {
        // Hash the submitted PIN and compare
        var hash = HashPin(Pin);
        if (hash != _configuredPinHash)
        {
            ErrorMessage = "Invalid PIN. Access denied.";
            return Page();
        }

        // Mark authenticated in server-side session.
        AdminGuard.MarkAuthenticated(HttpContext);

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
