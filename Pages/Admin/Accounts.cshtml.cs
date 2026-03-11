using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RodneyPortfolio.Models;
using RodneyPortfolio.Services;

namespace RodneyPortfolio.Pages.Admin;

public class AccountsModel : PageModel
{
    private readonly IAccountService _accountService;

    public AccountsModel(IAccountService accountService)
    {
        _accountService = accountService;
    }

    public List<ClientAccount> Accounts { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        if (!AdminGuard.IsAdminAuthenticated(HttpContext))
            return RedirectToPage("/Admin/AdminLogin");

        Accounts = await _accountService.GetAllAccountsAsync(ct);
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(string id, CancellationToken ct)
    {
        if (!AdminGuard.IsAdminAuthenticated(HttpContext))
            return RedirectToPage("/Admin/AdminLogin");

        if (!string.IsNullOrWhiteSpace(id))
            await _accountService.DeleteAccountAsync(id, ct);

        return RedirectToPage("/Admin/Accounts");
    }
}
