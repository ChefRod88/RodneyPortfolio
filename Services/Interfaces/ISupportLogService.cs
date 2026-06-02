using RodneyPortfolio.Models;

namespace RodneyPortfolio.Services;

public interface ISupportLogService
{
    Task LogAsync(PublicSupportRequestInput request, CancellationToken ct = default);
}
