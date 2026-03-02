using RodneyPortfolio.Models;

namespace RodneyPortfolio.Services;

public interface IQuoteSubmissionService
{
    Task SubmitAsync(QuoteRequestInput request, CancellationToken cancellationToken);
}
