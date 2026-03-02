using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RodneyPortfolio.Models;
using RodneyPortfolio.Services;

namespace RodneyPortfolio.Pages.Portal;

public class RegisterModel : PageModel
{
    private readonly IClientPortalService _portal;
    private readonly IPortalEmailService _email;
    private readonly ILogger<RegisterModel> _logger;

    public RegisterModel(IClientPortalService portal, IPortalEmailService email, ILogger<RegisterModel> logger)
    {
        _portal = portal;
        _email = email;
        _logger = logger;
    }

    [BindProperty] public RegisterInput Input { get; set; } = new();
    public string? ErrorMessage { get; private set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostRegisterAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid) return Page();

        if (await _portal.EmailExistsAsync(Input.Email, ct))
        {
            ErrorMessage = "An account with that email already exists. Please sign in instead.";
            return Page();
        }

        // Save pending account
        var account = new ClientAccount
        {
            FirstName = Input.FirstName,
            LastName = Input.LastName,
            Email = Input.Email,
            Phone = Input.Phone,
            CompanyName = Input.CompanyName,
            BillingAddress = Input.BillingAddress,
            City = Input.City,
            State = Input.State,
            ZipCode = Input.ZipCode,
            TierInterest = Input.TierInterest,
            Status = "Pending"
        };

        await _portal.SaveAccountAsync(account, ct);

        // Generate and send OTP
        var code = await _portal.GenerateOtpAsync(Input.Email, "register", ct);
        await _email.SendOtpAsync(Input.Email, Input.FirstName, code, "register", ct);

        _logger.LogInformation("Registration OTP sent to {Email}", Input.Email);

        return RedirectToPage("/Verify", new { email = Input.Email, purpose = "register" });
    }
}