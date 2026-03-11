using System.Text;
using RodneyPortfolio.Models;

namespace RodneyPortfolio.Services;

/// <summary>
/// Appends a sanitized quote request entry to a flat log file on disk.
/// Single responsibility: file I/O only — no email, no orchestration.
/// </summary>
public class QuoteLogService : IQuoteLogService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<QuoteLogService> _logger;

    public QuoteLogService(IWebHostEnvironment environment, ILogger<QuoteLogService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task LogAsync(QuoteRequestInput request, CancellationToken ct = default)
    {
        var dataDir = Path.Combine(_environment.ContentRootPath, "Data");
        Directory.CreateDirectory(dataDir);

        var entry = new StringBuilder()
            .AppendLine("========================================")
            .AppendLine($"SubmittedUtc: {DateTimeOffset.UtcNow:O}")
            .AppendLine($"Name: {QuoteSanitizer.Sanitize(request.Name)}")
            .AppendLine($"Email: {QuoteSanitizer.Sanitize(request.Email)}")
            .AppendLine($"Company: {QuoteSanitizer.Sanitize(request.Company)}")
            .AppendLine($"ServiceNeeded: {QuoteSanitizer.Sanitize(request.ServiceNeeded)}")
            .AppendLine($"EstimatedBudget: {QuoteSanitizer.Sanitize(request.EstimatedBudget)}")
            .AppendLine($"ProjectDescription: {QuoteSanitizer.Sanitize(request.ProjectDescription)}")
            .AppendLine($"Timeline: {QuoteSanitizer.Sanitize(request.Timeline)}")
            .AppendLine();

        await File.AppendAllTextAsync(
            Path.Combine(dataDir, "QuoteRequests.log"),
            entry.ToString(),
            ct);

        _logger.LogInformation("Quote request logged for {Email}", request.Email);
    }
}
