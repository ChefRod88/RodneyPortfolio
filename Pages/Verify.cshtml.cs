using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RodneyPortfolio.Services;

namespace RodneyPortfolio.Pages.Portal;

public class VerifyModel : PageModel
{
    private readonly IClientPortalService _portal;
    private readonly IPortalEmailService _email;

    public VerifyModel(IClientPortalService portal, IPortalEmailService email)
    {
        _portal = portal;
        _email = email;
    }

    public string Email { get; private set; } = string.Empty;
    public string Purpose { get; private set; } = "login";
    public string? ErrorMessage { get; private set; }

    public void OnGet(string email, string purpose = "login")
    {
        Email = email;
        Purpose = purpose;
    }

    public async Task<IActionResult> OnPostVerifyAsync(
        string email, string code, string purpose, CancellationToken ct)
    {
        Email = email;
        Purpose = purpose;

        var valid = await _portal.ValidateOtpAsync(email, code, purpose, ct);
        if (!valid)
        {
            ErrorMessage = "Invalid or expired code. Please try again or request a new one.";
            return Page();
        }

        var account = await _portal.GetByEmailAsync(email, ct);
        if (account is null)
        {
            ErrorMessage = "Account not found. Please register again.";
            return Page();
        }

        if (purpose == "register")
        {
            account.VerifiedAt = DateTimeOffset.UtcNow;
            account.Status = "Active";
            await _portal.SaveAccountAsync(account, ct);
            await _email.SendWelcomeAsync(account, ct);
        }

        // Update last login
        account.LastLoginAt = DateTimeOffset.UtcNow;
        await _portal.SaveAccountAsync(account, ct);

        // Create session cookie
        var session = await _portal.CreateSessionAsync(account.Id, account.Email, ct);
        Response.Cookies.Append("rc_portal_session", session.Id, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Expires = session.ExpiresAt
        });

        return RedirectToPage("/Dashboard");
    }

    public async Task<IActionResult> OnPostResendAsync(
        string email, string purpose, CancellationToken ct)
    {
        var account = await _portal.GetByEmailAsync(email, ct);
        if (account is not null)
        {
            var code = await _portal.GenerateOtpAsync(email, purpose, ct);
            await _email.SendOtpAsync(email, account.FirstName, code, purpose, ct);
        }
        Email = email;
        Purpose = purpose;
        return Page();
    }
}