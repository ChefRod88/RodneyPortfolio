using RodneyPortfolio.Models;

namespace RodneyPortfolio.Services;

public interface IInvoiceService
{
    Task<ClientRecord?> GetClientByEmailAsync(string email, CancellationToken ct = default);
    Task<List<Invoice>> GetInvoicesForClientAsync(string clientId, CancellationToken ct = default);
    Task<Invoice?> GetInvoiceByIdAsync(string invoiceId, CancellationToken ct = default);
    Task SaveInvoiceAsync(Invoice invoice, CancellationToken ct = default);
    Task SaveClientAsync(ClientRecord client, CancellationToken ct = default);
    Task<List<ClientRecord>> GetAllClientsAsync(CancellationToken ct = default);
    Task<List<Invoice>> GetAllInvoicesAsync(CancellationToken ct = default);
    Task MarkInvoicePaidAsync(string invoiceId, string stripePaymentIntentId, CancellationToken ct = default);
}