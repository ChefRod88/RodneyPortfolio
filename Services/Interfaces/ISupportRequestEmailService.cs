using RodneyPortfolio.Models;

namespace RodneyPortfolio.Services;

public interface ISupportRequestEmailService
{
    Task SendAsync(PublicSupportRequestInput request, CancellationToken ct = default);
}
