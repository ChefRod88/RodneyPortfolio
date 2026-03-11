using RodneyPortfolio.Models;

namespace RodneyPortfolio.Services;

/// <summary>
/// Sends a quote request notification email to the site owner.
/// </summary>
public interface IQuoteEmailService
{
    Task SendAsync(QuoteRequestInput request, CancellationToken ct = default);
}
