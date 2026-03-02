using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RodneyPortfolio.Models;
using RodneyPortfolio.Services;

namespace RodneyPortfolio.Pages.Admin;

public class AccountsModel : PageModel
{
    private readonly IClientPortalService _portalService;

    public AccountsModel(IClientPortalService portalService)
    {
        _portalService = portalService;
    }

    public List<ClientAccount> Accounts { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        if (!AdminGuard.IsAdminAuthenticated(HttpContext))
        {
            return RedirectToPage("/Admin/AdminLogin");
        }

        Accounts = await _portalService.GetAllAccountsAsync(ct);
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(string id, CancellationToken ct)
    {
        if (!AdminGuard.IsAdminAuthenticated(HttpContext))
        {
            return RedirectToPage("/Admin/AdminLogin");
        }

        if (!string.IsNullOrWhiteSpace(id))
        {
            await _portalService.DeleteAccountAsync(id, ct);
        }

        return RedirectToPage("/Admin/Accounts");
    }
}
