using System.Text;
using RodneyPortfolio.Models;

namespace RodneyPortfolio.Services;

public class SupportLogService : ISupportLogService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<SupportLogService> _logger;

    public SupportLogService(IWebHostEnvironment environment, ILogger<SupportLogService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task LogAsync(PublicSupportRequestInput request, CancellationToken ct = default)
    {
        try
        {
            var dataDir = Path.Combine(_environment.ContentRootPath, "Data");
            Directory.CreateDirectory(dataDir);

            var entry = new StringBuilder()
                .AppendLine("========================================")
                .AppendLine($"SubmittedUtc: {DateTimeOffset.UtcNow:O}")
                .AppendLine($"Name: {QuoteSanitizer.Sanitize(request.Name)}")
                .AppendLine($"Email: {QuoteSanitizer.Sanitize(request.Email)}")
                .AppendLine($"SiteOrProject: {QuoteSanitizer.Sanitize(request.SiteOrProject)}")
                .AppendLine($"Subject: {QuoteSanitizer.Sanitize(request.Subject)}")
                .AppendLine($"Message: {QuoteSanitizer.Sanitize(request.Message)}")
                .AppendLine();

            await File.AppendAllTextAsync(
                Path.Combine(dataDir, "SupportRequests.log"),
                entry.ToString(),
                ct);

            _logger.LogInformation("Support request logged for {Email}", request.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write support log for {Email}", request.Email);
        }
    }
}
