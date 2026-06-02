using RodneyPortfolio.Models;

namespace RodneyPortfolio.Services;

public class SupportRequestSubmissionService : ISupportRequestSubmissionService
{
    private readonly ISupportLogService _log;
    private readonly ISupportRequestEmailService _email;
    private readonly ILogger<SupportRequestSubmissionService> _logger;

    public SupportRequestSubmissionService(
        ISupportLogService log,
        ISupportRequestEmailService email,
        ILogger<SupportRequestSubmissionService> logger)
    {
        _log = log;
        _email = email;
        _logger = logger;
    }

    public async Task SubmitAsync(PublicSupportRequestInput request, CancellationToken cancellationToken)
    {
        try
        {
            await _log.LogAsync(request, cancellationToken);
            await _email.SendAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting support request for {Email}", request.Email);
            throw;
        }
    }
}
