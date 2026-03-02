using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using RodneyPortfolio.Models;
using RodneyPortfolio.Services;

namespace RodneyPortfolio.Pages.Portal;

public class DashboardModel : PageModel
{
    private readonly IClientPortalService _portal;
    private readonly IInvoiceService _invoiceService;
    private readonly IPortalEmailService _portalEmailService;
    private readonly StripeOptions _stripeOptions;

    public DashboardModel(
        IClientPortalService portal,
        IInvoiceService invoiceService,
        IPortalEmailService portalEmailService,
        IOptions<StripeOptions> stripeOptions)
    {
        _portal = portal;
        _invoiceService = invoiceService;
        _portalEmailService = portalEmailService;
        _stripeOptions = stripeOptions.Value;
    }

    public ClientAccount Account { get; private set; } = new();
    public List<Invoice> OpenInvoices { get; private set; } = new();
    public List<Invoice> PaidInvoices { get; private set; } = new();
    public decimal TotalDue => OpenInvoices.Sum(i => i.Amount);
    public decimal TotalPaid => PaidInvoices.Sum(i => i.Amount);
    public int UnpaidCount => OpenInvoices.Count;
    public string StripePublishableKey => _stripeOptions.PublishableKey;
    public string? StatusMessage { get; private set; }

    [BindProperty]
    public SupportMessageInput SupportMsg { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var account = await ResolveCurrentAccountAsync(ct);
        if (account is null)
        {
            return RedirectToPage("/Login");
        }

        await LoadDashboardAsync(account, ct);
        return Page();
    }

    public async Task<IActionResult> OnPostLogoutAsync(CancellationToken ct)
    {
        if (Request.Cookies.TryGetValue("rc_portal_session", out var sessionId) &&
            !string.IsNullOrWhiteSpace(sessionId))
        {
            await _portal.InvalidateSessionAsync(sessionId, ct);
        }

        Response.Cookies.Delete("rc_portal_session");
        return RedirectToPage("/Login");
    }

    public async Task<IActionResult> OnPostSupportAsync(CancellationToken ct)
    {
        var account = await ResolveCurrentAccountAsync(ct);
        if (account is null)
        {
            return RedirectToPage("/Login");
        }

        if (!ModelState.IsValid)
        {
            await LoadDashboardAsync(account, ct);
            return Page();
        }

        await _portalEmailService.SendSupportMessageAsync(account, SupportMsg, ct);
        StatusMessage = "Support message sent successfully.";
        SupportMsg = new SupportMessageInput();

        await LoadDashboardAsync(account, ct);
        return Page();
    }

    private async Task<ClientAccount?> ResolveCurrentAccountAsync(CancellationToken ct)
    {
        if (!Request.Cookies.TryGetValue("rc_portal_session", out var sessionId) ||
            string.IsNullOrWhiteSpace(sessionId))
        {
            return null;
        }

        var session = await _portal.GetSessionAsync(sessionId, ct);
        if (session is null)
        {
            return null;
        }

        return await _portal.GetByIdAsync(session.ClientId, ct);
    }

    private async Task LoadDashboardAsync(ClientAccount account, CancellationToken ct)
    {
        Account = account;

        var allInvoices = await _invoiceService.GetAllInvoicesAsync(ct);
        var accountInvoices = allInvoices
            .Where(i => string.Equals(i.ClientEmail, account.Email, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(i => i.IssuedAt)
            .ToList();

        OpenInvoices = accountInvoices
            .Where(i => i.Status is InvoiceStatus.Unpaid or InvoiceStatus.Overdue)
            .ToList();

        PaidInvoices = accountInvoices
            .Where(i => i.Status == InvoiceStatus.Paid)
            .ToList();
    }
}
