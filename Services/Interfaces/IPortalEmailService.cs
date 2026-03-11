using RodneyPortfolio.Models;

namespace RodneyPortfolio.Services;

public interface IPortalEmailService
{
    Task SendOtpAsync(string toEmail, string toName, string code, string purpose, CancellationToken ct = default);
    Task SendWelcomeAsync(ClientAccount account, CancellationToken ct = default);
    Task SendSupportMessageAsync(ClientAccount account, SupportMessageInput msg, CancellationToken ct = default);
    Task SendReceiptAsync(ClientAccount account, Invoice invoice, CancellationToken ct = default);
    Task SendCashAppPendingAsync(ClientAccount account, Invoice invoice, CancellationToken ct = default);
}
