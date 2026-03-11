using RodneyPortfolio.Models;

namespace RodneyPortfolio.Services;

/// <summary>
/// Appends quote request submissions to a persistent log file.
/// </summary>
public interface IQuoteLogService
{
    Task LogAsync(QuoteRequestInput request, CancellationToken ct = default);
}
