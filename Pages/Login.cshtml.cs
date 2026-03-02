using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RodneyPortfolio.Models;
using RodneyPortfolio.Services;

namespace RodneyPortfolio.Pages.Portal;

public class LoginModel : PageModel
{
    private readonly IClientPortalService _portal;
    private readonly IPortalEmailService _email;

    public LoginModel(IClientPortalService portal, IPortalEmailService email)
    {
        _portal = portal;
        _email = email;
    }

    [BindProperty] public LoginInput Input { get; set; } = new();
    public string? ErrorMessage { get; private set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostLoginAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid) return Page();

        var account = await _portal.GetByEmailAsync(Input.Email, ct);

        if (account is null)
        {
            ErrorMessage = "No account found with that email. Please register first.";
            return Page();
        }

        if (!account.IsVerified)
        {
            ErrorMessage = "Your account is not yet verified. Please complete registration first.";
            return Page();
        }

        var code = await _portal.GenerateOtpAsync(Input.Email, "login", ct);
        await _email.SendOtpAsync(Input.Email, account.FirstName, code, "login", ct);

        return RedirectToPage("/Verify", new { email = Input.Email, purpose = "login" });
    }
}
