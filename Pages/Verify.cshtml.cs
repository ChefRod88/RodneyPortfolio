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

    private const string SessionEmailKey   = "otp_email";
    private const string SessionPurposeKey = "otp_purpose";

    public void OnGet(string email, string purpose = "login")
    {
        Email   = email;
        Purpose = purpose;
        // Store in session so hidden inputs can't be tampered with
        HttpContext.Session.SetString(SessionEmailKey,   email);
        HttpContext.Session.SetString(SessionPurposeKey, purpose);
    }

    public async Task<IActionResult> OnPostVerifyAsync(
        string code, CancellationToken ct)
    {
        // Read from session — not from form — to prevent hidden-input tampering
        var email   = HttpContext.Session.GetString(SessionEmailKey)   ?? string.Empty;
        var purpose = HttpContext.Session.GetString(SessionPurposeKey) ?? "login";
        Email   = email;
        Purpose = purpose;

        if (string.IsNullOrWhiteSpace(email))
        {
            ErrorMessage = "Session expired. Please start again.";
            return Page();
        }

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

    public async Task<IActionResult> OnPostResendAsync(CancellationToken ct)
    {
        var email   = HttpContext.Session.GetString(SessionEmailKey)   ?? string.Empty;
        var purpose = HttpContext.Session.GetString(SessionPurposeKey) ?? "login";
        Email   = email;
        Purpose = purpose;

        if (!string.IsNullOrWhiteSpace(email))
        {
            var account = await _portal.GetByEmailAsync(email, ct);
            if (account is not null)
            {
                var code = await _portal.GenerateOtpAsync(email, purpose, ct);
                await _email.SendOtpAsync(email, account.FirstName, code, purpose, ct);
            }
        }
        return Page();
    }
}