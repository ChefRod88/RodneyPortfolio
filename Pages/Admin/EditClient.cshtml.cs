using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RodneyPortfolio.Models;
using RodneyPortfolio.Services;
using System.ComponentModel.DataAnnotations;

namespace RodneyPortfolio.Pages.Admin;

public class EditClientModel : PageModel
{
    private readonly IAccountService _accountService;

    public EditClientModel(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [BindProperty] public EditClientInput Input { get; set; } = new();
    public string? StatusMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(string id, CancellationToken ct)
    {
        if (!AdminGuard.IsAdminAuthenticated(HttpContext))
            return RedirectToPage("/Admin/AdminLogin");

        var account = await _accountService.GetByIdAsync(id, ct);
        if (account is null) return RedirectToPage("/Admin/Accounts");

        Input = new EditClientInput
        {
            Id             = account.Id,
            FirstName      = account.FirstName,
            LastName       = account.LastName,
            Email          = account.Email,
            Phone          = account.Phone,
            CompanyName    = account.CompanyName,
            BillingAddress = account.BillingAddress,
            City           = account.City,
            State          = account.State,
            ZipCode        = account.ZipCode,
            TierInterest   = account.TierInterest,
            Status         = account.Status
        };

        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync(CancellationToken ct)
    {
        if (!AdminGuard.IsAdminAuthenticated(HttpContext))
            return RedirectToPage("/Admin/AdminLogin");

        if (!ModelState.IsValid) return Page();

        var account = await _accountService.GetByIdAsync(Input.Id, ct);
        if (account is null) return RedirectToPage("/Admin/Accounts");

        account.FirstName      = Input.FirstName.Trim();
        account.LastName       = Input.LastName.Trim();
        account.Phone          = Input.Phone.Trim();
        account.CompanyName    = string.IsNullOrWhiteSpace(Input.CompanyName) ? null : Input.CompanyName.Trim();
        account.BillingAddress = Input.BillingAddress.Trim();
        account.City           = Input.City.Trim();
        account.State          = Input.State.Trim().ToUpper();
        account.ZipCode        = Input.ZipCode.Trim();
        account.TierInterest   = Input.TierInterest;
        account.Status         = Input.Status;

        await _accountService.UpdateAccountAsync(account, ct);
        StatusMessage = "Client updated successfully.";
        return Page();
    }
}

public class EditClientInput
{
    public string Id { get; set; } = string.Empty;

    [Required] public string FirstName { get; set; } = string.Empty;
    [Required] public string LastName { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string Phone { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    [Required] public string BillingAddress { get; set; } = string.Empty;
    [Required] public string City { get; set; } = string.Empty;
    [Required, MaxLength(2)] public string State { get; set; } = string.Empty;
    [Required] public string ZipCode { get; set; } = string.Empty;
    [Required] public string TierInterest { get; set; } = "Starter";
    [Required] public string Status { get; set; } = "Pending";
}
