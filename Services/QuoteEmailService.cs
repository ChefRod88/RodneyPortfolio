using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Options;
using RodneyPortfolio.Models;

namespace RodneyPortfolio.Services;

/// <summary>
/// Sends a quote request notification email to the site owner via SMTP.
/// Single responsibility: email delivery only — no file I/O, no orchestration.
/// </summary>
public class QuoteEmailService : IQuoteEmailService
{
    private readonly QuoteEmailOptions _options;
    private readonly ILogger<QuoteEmailService> _logger;

    public QuoteEmailService(IOptions<QuoteEmailOptions> options, ILogger<QuoteEmailService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(QuoteRequestInput request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_options.SmtpHost) ||
            string.IsNullOrWhiteSpace(_options.SmtpUsername) ||
            string.IsNullOrWhiteSpace(_options.SmtpPassword))
        {
            throw new InvalidOperationException(
                "Quote email is not configured. Set QuoteEmail SMTP settings.");
        }

        var fromEmail = string.IsNullOrWhiteSpace(_options.FromEmail)
            ? _options.SmtpUsername
            : _options.FromEmail;

        var safeName  = QuoteSanitizer.Sanitize(request.Name);
        var safeEmail = QuoteSanitizer.Sanitize(request.Email);

        var body = new StringBuilder()
            .AppendLine("New quote request submitted.")
            .AppendLine()
            .AppendLine($"Name: {safeName}")
            .AppendLine($"Email: {safeEmail}")
            .AppendLine($"Company: {QuoteSanitizer.Sanitize(request.Company)}")
            .AppendLine($"Service Needed: {QuoteSanitizer.Sanitize(request.ServiceNeeded)}")
            .AppendLine($"Estimated Budget: {QuoteSanitizer.Sanitize(request.EstimatedBudget)}")
            .AppendLine($"Project Description: {QuoteSanitizer.Sanitize(request.ProjectDescription)}")
            .AppendLine($"Timeline: {QuoteSanitizer.Sanitize(request.Timeline)}")
            .ToString();

        using var message = new MailMessage
        {
            From    = new MailAddress(fromEmail),
            Subject = $"New Quote Request - {safeName}",
            Body    = body
        };

        message.To.Add(_options.ToEmail);
        message.ReplyToList.Add(new MailAddress(safeEmail));

        try
        {
            using var client = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
            {
                EnableSsl   = _options.EnableSsl,
                Credentials = new NetworkCredential(_options.SmtpUsername, _options.SmtpPassword)
            };

            await client.SendMailAsync(message);
            _logger.LogInformation("Quote request email sent for {Email}", request.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send quote request email for {Email}", request.Email);
            throw;
        }
    }
}
