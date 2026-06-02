using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Options;
using RodneyPortfolio.Models;

namespace RodneyPortfolio.Services;

public class SupportRequestEmailService : ISupportRequestEmailService
{
    private readonly QuoteEmailOptions _options;
    private readonly ILogger<SupportRequestEmailService> _logger;

    public SupportRequestEmailService(IOptions<QuoteEmailOptions> options, ILogger<SupportRequestEmailService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(PublicSupportRequestInput request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_options.SmtpHost) ||
            string.IsNullOrWhiteSpace(_options.SmtpUsername) ||
            string.IsNullOrWhiteSpace(_options.SmtpPassword))
        {
            throw new InvalidOperationException(
                "Support email is not configured. Set QuoteEmail SMTP settings.");
        }

        var fromEmail = string.IsNullOrWhiteSpace(_options.FromEmail)
            ? _options.SmtpUsername
            : _options.FromEmail;

        var safeName = QuoteSanitizer.Sanitize(request.Name);
        var safeEmail = QuoteSanitizer.Sanitize(request.Email);

        var body = new StringBuilder()
            .AppendLine("New public support request submitted.")
            .AppendLine()
            .AppendLine($"Name: {safeName}")
            .AppendLine($"Email: {safeEmail}")
            .AppendLine($"Site/Project: {QuoteSanitizer.Sanitize(request.SiteOrProject)}")
            .AppendLine($"Subject: {QuoteSanitizer.Sanitize(request.Subject)}")
            .AppendLine()
            .AppendLine("Message:")
            .AppendLine(QuoteSanitizer.Sanitize(request.Message))
            .ToString();

        using var message = new MailMessage
        {
            From = new MailAddress(fromEmail),
            Subject = $"[RC Dev Support] {QuoteSanitizer.Sanitize(request.Subject)} — {safeName}",
            Body = body
        };

        message.To.Add(_options.ToEmail);
        message.ReplyToList.Add(new MailAddress(safeEmail));

        try
        {
            using var client = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
            {
                EnableSsl = _options.EnableSsl,
                Credentials = new NetworkCredential(_options.SmtpUsername, _options.SmtpPassword)
            };

            await client.SendMailAsync(message, ct);
            _logger.LogInformation("Support request email sent for {Email}", request.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send support request email for {Email}", request.Email);
            throw;
        }
    }
}
