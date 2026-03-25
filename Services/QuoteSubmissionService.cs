using RodneyPortfolio.Models;

namespace RodneyPortfolio.Services;

/// <summary>
/// Orchestrates a quote form submission: delegates logging to IQuoteLogService
/// and email delivery to IQuoteEmailService.
/// Single responsibility: coordination only — no I/O logic of its own.
/// </summary>
public class QuoteSubmissionService : IQuoteSubmissionService
{
    private readonly IQuoteLogService   _log;
    private readonly IQuoteEmailService _email;
    private readonly ILogger<QuoteSubmissionService> _logger;

    public QuoteSubmissionService(IQuoteLogService log, IQuoteEmailService email, ILogger<QuoteSubmissionService> logger)
    {
        _log   = log;
        _email = email;
        _logger = logger;
    }

    public async Task SubmitAsync(QuoteRequestInput request, CancellationToken cancellationToken)
    {
        try
        {
            await _log.LogAsync(request, cancellationToken);
            await _email.SendAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting quote request for {Email}", request.Email);
            throw;
        }
    }
}
