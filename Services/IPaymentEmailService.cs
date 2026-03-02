using RodneyPortfolio.Models;

namespace RodneyPortfolio.Services;

public interface IPaymentEmailService
{
    Task SendInvoiceEmailAsync(Invoice invoice, CancellationToken ct = default);
    Task SendPaymentConfirmationAsync(Invoice invoice, CancellationToken ct = default);
}
