using System.Net;
using System.Net.Mail;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Options;
using RodneyPortfolio.Models;

namespace RodneyPortfolio.Services;

public class PaymentEmailService : IPaymentEmailService
{
    private readonly QuoteEmailOptions _options;
    private readonly ILogger<PaymentEmailService> _logger;

    public PaymentEmailService(IOptions<QuoteEmailOptions> options, ILogger<PaymentEmailService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendInvoiceEmailAsync(Invoice invoice, CancellationToken ct = default)
    {
        if (!IsSmtpConfigured()) { _logger.LogWarning("SMTP not configured — skipping invoice email for {InvoiceId}", invoice.Id); return; }

        var safeName = HtmlEncoder.Default.Encode(invoice.ClientName);
        var safeDescription = HtmlEncoder.Default.Encode(invoice.Description);
        var shortInvoiceId = ToShortInvoiceId(invoice.Id);
        var portalUrl = $"https://www.rodneyachery.com/Payments?email={Uri.EscapeDataString(invoice.ClientEmail)}";
        var body = $"""
            <h2 style='color:#00d4ff;font-family:sans-serif'>Invoice from RC Dev</h2>
            <p>Hi {safeName},</p>
            <p>Your invoice for <strong>{safeDescription}</strong> is ready.</p>
            <table style='border-collapse:collapse;width:100%;max-width:400px'>
              <tr><td style='padding:8px;border:1px solid #ccc'><strong>Amount Due</strong></td><td style='padding:8px;border:1px solid #ccc'><strong>${invoice.Amount:F2}</strong></td></tr>
              <tr><td style='padding:8px;border:1px solid #ccc'>Invoice #</td><td style='padding:8px;border:1px solid #ccc'>{shortInvoiceId}</td></tr>
              <tr><td style='padding:8px;border:1px solid #ccc'>Due Date</td><td style='padding:8px;border:1px solid #ccc'>{invoice.DueAt:MMMM dd, yyyy}</td></tr>
            </table>
            <br/><a href='{portalUrl}' style='background:#00d4ff;color:#000;padding:12px 24px;text-decoration:none;font-weight:bold;display:inline-block'>View &amp; Pay Invoice →</a>
            <br/><br/><p style='font-size:12px;color:#666'>RC Dev · rodneyachery.com</p>
            """;
        await SendAsync(invoice.ClientEmail, $"Invoice #{shortInvoiceId} — ${invoice.Amount:F2} Due {invoice.DueAt:MMM dd}", body);
    }

    public async Task SendPaymentConfirmationAsync(Invoice invoice, CancellationToken ct = default)
    {
        if (!IsSmtpConfigured()) { _logger.LogWarning("SMTP not configured — skipping confirmation email for {InvoiceId}", invoice.Id); return; }

        var safeName = HtmlEncoder.Default.Encode(invoice.ClientName);
        var safeDescription = HtmlEncoder.Default.Encode(invoice.Description);
        var paidAt = invoice.PaidAt ?? DateTimeOffset.UtcNow;
        var body = $"""
            <h2 style='color:#00d4ff;font-family:sans-serif'>Payment Received ✓</h2>
            <p>Hi {safeName},</p>
            <p>We received your payment of <strong>${invoice.Amount:F2}</strong> for <strong>{safeDescription}</strong>.</p>
            <p>Confirmed: {paidAt:MMMM dd, yyyy 'at' h:mm tt} UTC</p>
            <p>Thank you for your business!</p>
            <p style='font-size:12px;color:#666'>RC Dev · rodneyachery.com</p>
            """;
        await SendAsync(invoice.ClientEmail, $"Payment Confirmed — ${invoice.Amount:F2}", body);
        if (!string.IsNullOrWhiteSpace(_options.ToEmail))
        {
            await SendAsync(_options.ToEmail, $"[RC Dev] Payment received from {safeName} — ${invoice.Amount:F2}", body);
        }
    }

    private async Task SendAsync(string toEmail, string subject, string htmlBody)
    {
        try
        {
            var from = string.IsNullOrWhiteSpace(_options.FromEmail) ? _options.SmtpUsername : _options.FromEmail;
            _logger.LogInformation("Sending payment email to {ToEmail}: {Subject}", toEmail, subject);
            using var msg = new MailMessage { From = new MailAddress(from, "RC Dev"), Subject = subject, Body = htmlBody, IsBodyHtml = true };
            msg.To.Add(toEmail);
            using var smtp = new SmtpClient(_options.SmtpHost, _options.SmtpPort) { EnableSsl = _options.EnableSsl, Credentials = new NetworkCredential(_options.SmtpUsername, _options.SmtpPassword) };
            await smtp.SendMailAsync(msg);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send payment email to {ToEmail}: {Subject}", toEmail, subject);
        }
    }

    private bool IsSmtpConfigured() =>
        !string.IsNullOrWhiteSpace(_options.SmtpHost) &&
        !string.IsNullOrWhiteSpace(_options.SmtpUsername) &&
        !string.IsNullOrWhiteSpace(_options.SmtpPassword);

    private static string ToShortInvoiceId(string invoiceId)
    {
        if (string.IsNullOrWhiteSpace(invoiceId))
        {
            return "UNKNOWN";
        }

        return invoiceId.Length <= 8 ? invoiceId.ToUpperInvariant() : invoiceId[..8].ToUpperInvariant();
    }
}