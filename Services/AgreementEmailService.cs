using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Options;
using RodneyPortfolio.Models;

namespace RodneyPortfolio.Services;

/// <summary>
/// Service to send signed PDF agreements via SMTP email to Rodney and CC the client.
/// </summary>
public class AgreementEmailService : IAgreementEmailService
{
    private readonly QuoteEmailOptions _options;
    private readonly ILogger<AgreementEmailService> _logger;

    public AgreementEmailService(IOptions<QuoteEmailOptions> options, ILogger<AgreementEmailService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(AgreementSubmissionInput input, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_options.SmtpHost) ||
            string.IsNullOrWhiteSpace(_options.SmtpUsername) ||
            string.IsNullOrWhiteSpace(_options.SmtpPassword))
        {
            throw new InvalidOperationException("SMTP email configuration is missing or incomplete.");
        }

        var fromEmail = string.IsNullOrWhiteSpace(_options.FromEmail)
            ? _options.SmtpUsername
            : _options.FromEmail;

        var body = new StringBuilder()
            .AppendLine("A new Software Development Services Agreement has been signed on the website.")
            .AppendLine()
            .AppendLine($"Client Name: {QuoteSanitizer.Sanitize(input.ClientName)}")
            .AppendLine($"Client Email: {QuoteSanitizer.Sanitize(input.ClientEmail)}")
            .AppendLine($"Company Name: {QuoteSanitizer.Sanitize(input.ClientCompany)}")
            .AppendLine($"Date (UTC):   {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}")
            .AppendLine($"IP Address:   {QuoteSanitizer.Sanitize(input.ClientIp)}")
            .AppendLine()
            .AppendLine("The fully signed PDF contract is attached to this email.")
            .ToString();

        using var message = new MailMessage
        {
            From = new MailAddress(fromEmail),
            Subject = $"Signed Contract - {QuoteSanitizer.Sanitize(input.ClientCompany)}",
            Body = body
        };

        message.To.Add(_options.ToEmail);
        message.CC.Add(input.ClientEmail);

        // Convert the Base64 string back to bytes and attach
        byte[] pdfBytes = Convert.FromBase64String(input.PdfBase64);
        var ms = new MemoryStream(pdfBytes);
        var cleanFileName = $"Software_Services_Agreement_{QuoteSanitizer.Sanitize(input.ClientCompany).Replace(" ", "_")}.pdf";
        
        var attachment = new Attachment(ms, cleanFileName, "application/pdf");
        message.Attachments.Add(attachment);

        try
        {
            using var client = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
            {
                EnableSsl = _options.EnableSsl,
                Credentials = new NetworkCredential(_options.SmtpUsername, _options.SmtpPassword)
            };

            await client.SendMailAsync(message, ct);
            _logger.LogInformation("Agreement PDF email sent successfully for {Company}", input.ClientCompany);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send signed agreement email to {Email}", input.ClientEmail);
            throw;
        }
        finally
        {
            // Close the MemoryStream after MailMessage is sent and disposed
            ms.Close();
        }
    }
}
