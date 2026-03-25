using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RodneyPortfolio.Models;
using RodneyPortfolio.Services;

namespace RodneyPortfolio.Pages.Admin;

public class AccountsModel : PageModel
{
    private readonly IAccountService _accountService;
    private readonly ILogger<AccountsModel> _logger;

    public AccountsModel(IAccountService accountService, ILogger<AccountsModel> logger)
    {
        _accountService = accountService;
        _logger = logger;
    }

    public List<ClientAccount> Accounts { get; private set; } = new();
    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        if (!AdminGuard.IsAdminAuthenticated(HttpContext))
            return RedirectToPage("/Admin/AdminLogin");

        try
        {
            Accounts = await _accountService.GetAllAccountsAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading accounts list");
            ErrorMessage = "Failed to load accounts. Please try again.";
        }

        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(string id, CancellationToken ct)
    {
        if (!AdminGuard.IsAdminAuthenticated(HttpContext))
            return RedirectToPage("/Admin/AdminLogin");

        try
        {
            if (!string.IsNullOrWhiteSpace(id))
                await _accountService.DeleteAccountAsync(id, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting account {Id}", id);
            TempData["Error"] = "Failed to delete account. Please try again.";
        }

        return RedirectToPage("/Admin/Accounts");
    }
}
