using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using RodneyPortfolio.Models;
using RodneyPortfolio.Services;
using Stripe;
using InvoiceModel = RodneyPortfolio.Models.Invoice;

namespace RodneyPortfolio.Pages;

public class PaymentsModel : PageModel
{
    private readonly IInvoiceService _invoiceService;
    private readonly IPaymentEmailService _emailService;
    private readonly ILogger<PaymentsModel> _logger;
    private readonly StripeOptions _stripe;

    public PaymentsModel(IInvoiceService invoiceService, IPaymentEmailService emailService,
        IOptions<StripeOptions> stripe, ILogger<PaymentsModel> logger)
    {
        _invoiceService = invoiceService;
        _emailService = emailService;
        _logger = logger;
        _stripe = stripe.Value;
    }

    public string StripePublishableKey => _stripe.PublishableKey;
    public ClientRecord? Client { get; private set; }
    public List<InvoiceModel>? Invoices { get; private set; }
    public string SearchEmail { get; private set; } = string.Empty;
    public bool IsClientNotFound { get; private set; }
    public decimal TotalDue => Invoices?.Where(i => i.Status is InvoiceStatus.Unpaid or InvoiceStatus.Overdue).Sum(i => i.Amount) ?? 0;
    public decimal TotalPaid => Invoices?.Where(i => i.Status == InvoiceStatus.Paid).Sum(i => i.Amount) ?? 0;
    public int UnpaidCount => Invoices?.Count(i => i.Status is InvoiceStatus.Unpaid or InvoiceStatus.Overdue) ?? 0;

    public async Task OnGetAsync(string? email, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(email)) await LookupAsync(email, ct);
    }

    public async Task<IActionResult> OnPostLookupAsync([FromForm] string email, CancellationToken ct)
    {
        await LookupAsync(email, ct); return Page();
    }

    private async Task LookupAsync(string email, CancellationToken ct)
    {
        SearchEmail = (email ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(SearchEmail))
        {
            IsClientNotFound = true;
            return;
        }

        Client = await _invoiceService.GetClientByEmailAsync(SearchEmail, ct);
        if (Client is null) { IsClientNotFound = true; return; }
        Invoices = await _invoiceService.GetInvoicesForClientAsync(Client.Id, ct);
    }

    public async Task<IActionResult> OnPostCreatePaymentIntentAsync([FromBody] CreatePaymentIntentRequest req, CancellationToken ct)
    {
        if (req is null || string.IsNullOrWhiteSpace(req.InvoiceId))
        {
            return BadRequest(new { error = "Missing invoice id." });
        }

        if (string.IsNullOrWhiteSpace(_stripe.SecretKey))
        {
            _logger.LogError("Stripe secret key is not configured.");
            return StatusCode(500, new { error = "Payment provider is not configured." });
        }

        var invoice = await _invoiceService.GetInvoiceByIdAsync(req.InvoiceId, ct);
        if (invoice is null || invoice.Status == InvoiceStatus.Paid)
            return BadRequest(new { error = "Invoice not found or already paid." });

        StripeConfiguration.ApiKey = _stripe.SecretKey;
        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(invoice.Amount * 100),
            Currency = "usd",
            Description = $"RC Dev – {invoice.Description}",
            ReceiptEmail = invoice.ClientEmail,
            Metadata = new Dictionary<string, string> { ["invoiceId"] = invoice.Id },
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions { Enabled = true }
        };
        var intent = await new PaymentIntentService().CreateAsync(options, cancellationToken: ct);
        invoice.StripePaymentIntentId = intent.Id;
        await _invoiceService.SaveInvoiceAsync(invoice, ct);
        return new JsonResult(new { clientSecret = intent.ClientSecret });
    }

    public async Task<IActionResult> OnPostConfirmPaymentAsync([FromBody] ConfirmPaymentRequest req, CancellationToken ct)
    {
        try
        {
            if (req is null || string.IsNullOrWhiteSpace(req.InvoiceId) || string.IsNullOrWhiteSpace(req.PaymentIntentId))
            {
                return BadRequest(new { ok = false, error = "Missing payment confirmation payload." });
            }

            if (string.IsNullOrWhiteSpace(_stripe.SecretKey))
            {
                _logger.LogError("Stripe secret key is not configured.");
                return StatusCode(500, new { ok = false, error = "Payment provider is not configured." });
            }

            var invoice = await _invoiceService.GetInvoiceByIdAsync(req.InvoiceId, ct);
            if (invoice is null)
            {
                return BadRequest(new { ok = false, error = "Invoice not found." });
            }

            if (invoice.Status == InvoiceStatus.Paid)
            {
                return new JsonResult(new { ok = true, alreadyPaid = true });
            }

            StripeConfiguration.ApiKey = _stripe.SecretKey;
            var paymentIntent = await new PaymentIntentService().GetAsync(req.PaymentIntentId, cancellationToken: ct);
            if (paymentIntent is null ||
                !string.Equals(paymentIntent.Id, req.PaymentIntentId, StringComparison.Ordinal) ||
                paymentIntent.Status != "succeeded")
            {
                return BadRequest(new { ok = false, error = "Payment is not yet completed." });
            }

            if (paymentIntent.Currency is not "usd")
            {
                return BadRequest(new { ok = false, error = "Unexpected payment currency." });
            }

            var expectedAmount = (long)(invoice.Amount * 100);
            if (paymentIntent.AmountReceived < expectedAmount)
            {
                return BadRequest(new { ok = false, error = "Payment amount does not cover this invoice." });
            }

            if (paymentIntent.Metadata is null ||
                !paymentIntent.Metadata.TryGetValue("invoiceId", out var invoiceIdFromMetadata) ||
                !string.Equals(invoiceIdFromMetadata, invoice.Id, StringComparison.Ordinal))
            {
                return BadRequest(new { ok = false, error = "Payment invoice verification failed." });
            }

            await _invoiceService.MarkInvoicePaidAsync(req.InvoiceId, req.PaymentIntentId, ct);
            invoice.Status = InvoiceStatus.Paid;
            invoice.PaidAt = DateTimeOffset.UtcNow;
            await _emailService.SendPaymentConfirmationAsync(invoice, ct);
            return new JsonResult(new { ok = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to confirm payment for {InvoiceId}", req.InvoiceId);
            return StatusCode(500, new { ok = false });
        }
    }
}

