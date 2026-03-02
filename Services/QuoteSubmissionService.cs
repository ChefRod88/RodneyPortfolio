using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Options;
using RodneyPortfolio.Models;

namespace RodneyPortfolio.Services;

public class QuoteSubmissionService : IQuoteSubmissionService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<QuoteSubmissionService> _logger;
    private readonly QuoteEmailOptions _options;

    public QuoteSubmissionService(
        IWebHostEnvironment environment,
        IOptions<QuoteEmailOptions> options,
        ILogger<QuoteSubmissionService> logger)
    {
        _environment = environment;
        _logger = logger;
        _options = options.Value;
    }

    public async Task SubmitAsync(QuoteRequestInput request, CancellationToken cancellationToken)
    {
        await AppendSubmissionLogAsync(request, cancellationToken);
        await SendEmailAsync(request);
    }

    private async Task AppendSubmissionLogAsync(QuoteRequestInput request, CancellationToken cancellationToken)
    {
        var dataDir = Path.Combine(_environment.ContentRootPath, "Data");
        Directory.CreateDirectory(dataDir);

        var logPath = Path.Combine(dataDir, "QuoteRequests.log");
        var now = DateTimeOffset.UtcNow;

        var entry = new StringBuilder()
            .AppendLine("========================================")
            .AppendLine($"SubmittedUtc: {now:O}")
            .AppendLine($"Name: {request.Name}")
            .AppendLine($"Email: {request.Email}")
            .AppendLine($"Company: {request.Company ?? string.Empty}")
            .AppendLine($"ServiceNeeded: {request.ServiceNeeded}")
            .AppendLine($"EstimatedBudget: {request.EstimatedBudget}")
            .AppendLine($"ProjectDescription: {request.ProjectDescription}")
            .AppendLine($"Timeline: {request.Timeline ?? string.Empty}")
            .AppendLine();

        await File.AppendAllTextAsync(logPath, entry.ToString(), cancellationToken);
    }

    private async Task SendEmailAsync(QuoteRequestInput request)
    {
        if (string.IsNullOrWhiteSpace(_options.SmtpHost) ||
            string.IsNullOrWhiteSpace(_options.SmtpUsername) ||
            string.IsNullOrWhiteSpace(_options.SmtpPassword))
        {
            throw new InvalidOperationException("Quote email is not configured. Set QuoteEmail SMTP settings.");
        }

        var fromEmail = string.IsNullOrWhiteSpace(_options.FromEmail)
            ? _options.SmtpUsername
            : _options.FromEmail;

        var body = new StringBuilder()
            .AppendLine("New quote request submitted.")
            .AppendLine()
            .AppendLine($"Name: {request.Name}")
            .AppendLine($"Email: {request.Email}")
            .AppendLine($"Company: {request.Company ?? string.Empty}")
            .AppendLine($"Service Needed: {request.ServiceNeeded}")
            .AppendLine($"Estimated Budget: {request.EstimatedBudget}")
            .AppendLine($"Project Description: {request.ProjectDescription}")
            .AppendLine($"Timeline: {request.Timeline ?? string.Empty}")
            .ToString();

        using var message = new MailMessage
        {
            From = new MailAddress(fromEmail),
            Subject = $"New Quote Request - {request.Name}",
            Body = body
        };

        message.To.Add(_options.ToEmail);
        message.ReplyToList.Add(new MailAddress(request.Email));

        using var client = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
        {
            EnableSsl = _options.EnableSsl,
            Credentials = new NetworkCredential(_options.SmtpUsername, _options.SmtpPassword)
        };

        await client.SendMailAsync(message);
        _logger.LogInformation("Quote request email sent for {Email}", request.Email);
    }
}
