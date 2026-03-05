using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using RodneyPortfolio.Models;
using RodneyPortfolio.Services;
using Stripe;
using InvoiceModel = RodneyPortfolio.Models.Invoice;

namespace RodneyPortfolio.Pages.Portal;

public class DashboardModel : PageModel
{
    private const string DevBypassSessionKey = "rc_dev_bypass_email";

    private readonly IClientPortalService _portal;
    private readonly IInvoiceService _invoiceService;
    private readonly IPortalEmailService _portalEmailService;
    private readonly StripeOptions _stripeOptions;
    private readonly IWebHostEnvironment _env;

    public DashboardModel(
        IClientPortalService portal,
        IInvoiceService invoiceService,
        IPortalEmailService portalEmailService,
        IOptions<StripeOptions> stripeOptions,
        IWebHostEnvironment env)
    {
        _portal = portal;
        _invoiceService = invoiceService;
        _portalEmailService = portalEmailService;
        _stripeOptions = stripeOptions.Value;
        _env = env;
    }

    public ClientAccount Account { get; private set; } = new();
    public List<InvoiceModel> OpenInvoices { get; private set; } = new();
    public List<InvoiceModel> PaidInvoices { get; private set; } = new();
    public decimal TotalDue => OpenInvoices.Sum(i => i.Amount);
    public decimal TotalPaid => PaidInvoices.Sum(i => i.Amount);
    public int UnpaidCount => OpenInvoices.Count;
    public string StripePublishableKey => _stripeOptions.PublishableKey;
    public string? StatusMessage { get; private set; }

    [BindProperty]
    public SupportMessageInput SupportMsg { get; set; } = new();

    // ── GET ───────────────────────────────────────────────────────────────────
    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var account = await ResolveCurrentAccountAsync(ct);
        if (account is null)
            return RedirectToPage("/Login");

        await LoadDashboardAsync(account, ct);
        return Page();
    }

    // ── LOGOUT ────────────────────────────────────────────────────────────────
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

    // ── SUPPORT ───────────────────────────────────────────────────────────────
    public async Task<IActionResult> OnPostSupportAsync(CancellationToken ct)
    {
        var account = await ResolveCurrentAccountAsync(ct);
        if (account is null)
            return RedirectToPage("/Login");

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

    // ── STRIPE: CREATE PAYMENT INTENT ─────────────────────────────────────────
    public async Task<IActionResult> OnPostCreatePaymentIntentAsync(
        [FromBody] CreatePaymentIntentRequest req, CancellationToken ct)
    {
        var account = await ResolveCurrentAccountAsync(ct);
        if (account is null)
            return new JsonResult(new { error = "Unauthorized" }) { StatusCode = 401 };

        var allInvoices = await _invoiceService.GetAllInvoicesAsync(ct);
        var invoice = allInvoices.FirstOrDefault(i =>
            i.Id == req.InvoiceId &&
            string.Equals(i.ClientEmail, account.Email, StringComparison.OrdinalIgnoreCase));

        if (invoice is null)
            return new JsonResult(new { error = "Invoice not found" }) { StatusCode = 404 };

        StripeConfiguration.ApiKey = _stripeOptions.SecretKey;

        var options = new PaymentIntentCreateOptions
        {
            Amount   = (long)(invoice.Amount * 100), // convert dollars → cents
            Currency = "usd",
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true
            },
            Metadata = new Dictionary<string, string>
            {
                ["invoiceId"]     = invoice.Id,
                ["clientEmail"]   = account.Email,
                ["invoiceNumber"] = invoice.InvoiceNumber ?? invoice.Id[..8].ToUpper()
            }
        };

        var service = new PaymentIntentService();
        var intent  = await service.CreateAsync(options, cancellationToken: ct);

        return new JsonResult(new { clientSecret = intent.ClientSecret });
    }

    // ── STRIPE: CONFIRM PAYMENT ───────────────────────────────────────────────
    public async Task<IActionResult> OnPostConfirmPaymentAsync(
        [FromBody] ConfirmPaymentRequest req, CancellationToken ct)
    {
        try
        {
            var account = await ResolveCurrentAccountAsync(ct);
            if (account is null)
                return new JsonResult(new { error = "Unauthorized" }) { StatusCode = 401 };

            var allInvoices = await _invoiceService.GetAllInvoicesAsync(ct);
            var invoice = allInvoices.FirstOrDefault(i =>
                i.Id == req.InvoiceId &&
                string.Equals(i.ClientEmail, account.Email, StringComparison.OrdinalIgnoreCase));

            if (invoice is null)
                return new JsonResult(new { error = "Invoice not found" }) { StatusCode = 404 };

            StripeConfiguration.ApiKey = _stripeOptions.SecretKey;
            var paymentIntent = await new PaymentIntentService().GetAsync(req.PaymentIntentId, cancellationToken: ct);

            if (paymentIntent is null || paymentIntent.Status != "succeeded")
                return new JsonResult(new { error = "Payment is not yet completed." }) { StatusCode = 400 };

            if (paymentIntent.Currency is not "usd")
                return new JsonResult(new { error = "Unexpected payment currency." }) { StatusCode = 400 };

            if (paymentIntent.AmountReceived < (long)(invoice.Amount * 100))
                return new JsonResult(new { error = "Payment amount does not cover this invoice." }) { StatusCode = 400 };

            if (paymentIntent.Metadata is null ||
                !paymentIntent.Metadata.TryGetValue("invoiceId", out var metaInvoiceId) ||
                !string.Equals(metaInvoiceId, invoice.Id, StringComparison.Ordinal))
                return new JsonResult(new { error = "Payment invoice verification failed." }) { StatusCode = 400 };

            invoice.Status                = InvoiceStatus.Paid;
            invoice.PaidAt                = DateTime.UtcNow;
            invoice.StripePaymentIntentId = req.PaymentIntentId;
            invoice.PaymentMethod         = "Stripe";

            await _invoiceService.UpdateInvoiceAsync(invoice, ct);

            // Fire-and-forget — don't block the JSON response
            _ = _portalEmailService.SendReceiptAsync(account, invoice, ct);

            return new JsonResult(new { success = true });
        }
        catch (Exception ex)
        {
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<DashboardModel>>();
            logger.LogError(ex, "Failed to confirm payment for {InvoiceId}", req.InvoiceId);
            return new JsonResult(new { error = "Payment confirmation failed." }) { StatusCode = 500 };
        }
    }

    // ── CASH APP: MARK PENDING ────────────────────────────────────────────────
    public async Task<IActionResult> OnPostCashAppPendingAsync(
        [FromBody] CashAppPendingRequest req, CancellationToken ct)
    {
        var account = await ResolveCurrentAccountAsync(ct);
        if (account is null)
            return new JsonResult(new { error = "Unauthorized" }) { StatusCode = 401 };

        var allInvoices = await _invoiceService.GetAllInvoicesAsync(ct);
        var invoice = allInvoices.FirstOrDefault(i =>
            i.Id == req.InvoiceId &&
            string.Equals(i.ClientEmail, account.Email, StringComparison.OrdinalIgnoreCase));

        if (invoice is null)
            return new JsonResult(new { error = "Invoice not found" }) { StatusCode = 404 };

        invoice.Status        = InvoiceStatus.PendingCashApp;
        invoice.PaymentMethod = "CashApp";

        await _invoiceService.UpdateInvoiceAsync(invoice, ct);

        // Fire-and-forget admin notification
        _ = _portalEmailService.SendCashAppPendingAsync(account, invoice, ct);

        return new JsonResult(new { success = true });
    }

    // ── PRIVATE HELPERS ───────────────────────────────────────────────────────
    private async Task<ClientAccount?> ResolveCurrentAccountAsync(CancellationToken ct)
    {
        if (_env.IsDevelopment())
        {
            var queryBypass = Request.Query["devBypassEmail"].ToString().Trim();
            if (!string.IsNullOrWhiteSpace(queryBypass))
            {
                HttpContext.Session.SetString(DevBypassSessionKey, queryBypass);
                return await EnsureDevelopmentBypassAccountAsync(queryBypass, ct);
            }

            var sessionBypass = HttpContext.Session.GetString(DevBypassSessionKey);
            if (!string.IsNullOrWhiteSpace(sessionBypass))
            {
                return await EnsureDevelopmentBypassAccountAsync(sessionBypass, ct);
            }
        }

        if (!Request.Cookies.TryGetValue("rc_portal_session", out var sessionId) ||
            string.IsNullOrWhiteSpace(sessionId))
            return null;

        var session = await _portal.GetSessionAsync(sessionId, ct);
        if (session is null)
            return null;

        return await _portal.GetByIdAsync(session.ClientId, ct);
    }

    private async Task<ClientAccount?> EnsureDevelopmentBypassAccountAsync(string email, CancellationToken ct)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalizedEmail))
        {
            return null;
        }

        var account = await _portal.GetByEmailAsync(normalizedEmail, ct);
        if (account is null)
        {
            account = new ClientAccount
            {
                FirstName = "Dev",
                LastName = "Tester",
                Email = normalizedEmail,
                Phone = "555-0100",
                CompanyName = "Local QA",
                BillingAddress = "123 Test Street",
                City = "Testville",
                State = "TX",
                ZipCode = "75001",
                TierInterest = "Starter",
                Status = "Active"
            };

            await _portal.SaveAccountAsync(account, ct);
        }

        var existingInvoices = await _invoiceService.GetAllInvoicesAsync(ct);
        var hasOpenInvoice = existingInvoices.Any(i =>
            string.Equals(i.ClientEmail, account.Email, StringComparison.OrdinalIgnoreCase) &&
            i.Status is InvoiceStatus.Unpaid or InvoiceStatus.Overdue or InvoiceStatus.PendingCashApp);

        if (!hasOpenInvoice)
        {
            await _invoiceService.SaveInvoiceAsync(new InvoiceModel
            {
                ClientId = account.Id,
                ClientName = account.FullName,
                ClientEmail = account.Email,
                Description = "Development Stripe flow validation",
                Amount = 42.42m,
                IssuedAt = DateTimeOffset.UtcNow,
                DueAt = DateTimeOffset.UtcNow.AddDays(7),
                Status = InvoiceStatus.Unpaid,
                InvoiceNumber = $"DEV-{DateTimeOffset.UtcNow:yyyyMMddHHmm}"
            }, ct);
        }

        return account;
    }

    private async Task LoadDashboardAsync(ClientAccount account, CancellationToken ct)
    {
        Account = account;

        var allInvoices = await _invoiceService.GetAllInvoicesAsync(ct);
        var accountInvoices = allInvoices
            .Where(i => string.Equals(i.ClientEmail, account.Email, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(i => i.IssuedAt)
            .ToList();

        // PendingCashApp invoices stay in the "open" list so the client can see them
        OpenInvoices = accountInvoices
            .Where(i => i.Status is InvoiceStatus.Unpaid
                                 or InvoiceStatus.Overdue
                                 or InvoiceStatus.PendingCashApp)
            .ToList();

        PaidInvoices = accountInvoices
            .Where(i => i.Status == InvoiceStatus.Paid)
            .ToList();
    }
}

