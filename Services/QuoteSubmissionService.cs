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

    public QuoteSubmissionService(IQuoteLogService log, IQuoteEmailService email)
    {
        _log   = log;
        _email = email;
    }

    public async Task SubmitAsync(QuoteRequestInput request, CancellationToken cancellationToken)
    {
        await _log.LogAsync(request, cancellationToken);
        await _email.SendAsync(request, cancellationToken);
    }
}
