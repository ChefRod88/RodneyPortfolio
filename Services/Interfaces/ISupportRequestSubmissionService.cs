using RodneyPortfolio.Models;

namespace RodneyPortfolio.Services;

public interface ISupportRequestSubmissionService
{
    Task SubmitAsync(PublicSupportRequestInput request, CancellationToken cancellationToken);
}
