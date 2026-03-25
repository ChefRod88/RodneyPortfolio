using Microsoft.EntityFrameworkCore;
using RodneyPortfolio.Data;
using RodneyPortfolio.Models;

namespace RodneyPortfolio.Services;

public class SqlInvoiceService : IInvoiceService
{
    private readonly AppDbContext _db;
    private readonly ILogger<SqlInvoiceService> _logger;

    public SqlInvoiceService(AppDbContext db, ILogger<SqlInvoiceService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<ClientRecord?> GetClientByEmailAsync(string email, CancellationToken ct = default)
    {
        try
        {
            var account = await _db.ClientAccounts
                .FirstOrDefaultAsync(a => a.Email.ToLower() == email.ToLower(), ct);
            if (account is null) return null;
            return new ClientRecord
            {
                Id = account.Id,
                Name = account.FullName,
                Email = account.Email
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving client by email {Email}", email);
            throw;
        }
    }

    public async Task<List<Invoice>> GetInvoicesForClientAsync(string clientId, CancellationToken ct = default)
    {
        try
        {
            return await _db.Invoices
                .Where(i => i.ClientId == clientId)
                .OrderByDescending(i => i.IssuedAt)
                .ToListAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving invoices for client {ClientId}", clientId);
            throw;
        }
    }

    public async Task<Invoice?> GetInvoiceByIdAsync(string id, CancellationToken ct = default)
    {
        try
        {
            return await _db.Invoices.FindAsync(new object[] { id }, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving invoice {InvoiceId}", id);
            throw;
        }
    }

    public async Task SaveInvoiceAsync(Invoice invoice, CancellationToken ct = default)
    {
        try
        {
            var exists = await _db.Invoices.AnyAsync(i => i.Id == invoice.Id, ct);
            if (exists)
                _db.Invoices.Update(invoice);
            else
                await _db.Invoices.AddAsync(invoice, ct);
            await _db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving invoice {InvoiceId}", invoice.Id);
            throw;
        }
    }

    public async Task SaveClientAsync(ClientRecord client, CancellationToken ct = default)
    {
        try
        {
            // ClientRecord is a lightweight view over ClientAccount
            // For invoice purposes, just verify account exists
            var exists = await _db.ClientAccounts.AnyAsync(a => a.Id == client.Id, ct);
            if (!exists)
            {
                await _db.ClientAccounts.AddAsync(new ClientAccount
                {
                    Id = client.Id,
                    FirstName = client.Name.Split(' ').FirstOrDefault() ?? client.Name,
                    LastName = client.Name.Split(' ').Skip(1).FirstOrDefault() ?? "",
                    Email = client.Email,
                    Phone = "N/A",
                    BillingAddress = "N/A",
                    City = "N/A",
                    State = "N/A",
                    ZipCode = "N/A"
                }, ct);
                await _db.SaveChangesAsync(ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving client record {ClientId}", client.Id);
            throw;
        }
    }

    public async Task<List<ClientRecord>> GetAllClientsAsync(CancellationToken ct = default)
    {
        try
        {
            var accounts = await _db.ClientAccounts.ToListAsync(ct);
            return accounts.Select(a => new ClientRecord
            {
                Id = a.Id,
                Name = a.FullName,
                Email = a.Email
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all clients");
            throw;
        }
    }

    public async Task<List<Invoice>> GetAllInvoicesAsync(CancellationToken ct = default)
    {
        try
        {
            return await _db.Invoices
                .OrderByDescending(i => i.IssuedAt)
                .ToListAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all invoices");
            throw;
        }
    }

    public async Task UpdateInvoiceAsync(Invoice invoice, CancellationToken ct = default)
    {
        try
        {
            _db.Invoices.Update(invoice);
            await _db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating invoice {InvoiceId}", invoice.Id);
            throw;
        }
    }

    public async Task MarkInvoicePaidAsync(string invoiceId, string stripePaymentIntentId, CancellationToken ct = default)
    {
        try
        {
            var invoice = await _db.Invoices.FindAsync(new object[] { invoiceId }, ct);
            if (invoice is not null)
            {
                invoice.Status                = InvoiceStatus.Paid;
                invoice.PaidAt                = DateTimeOffset.UtcNow;
                invoice.StripePaymentIntentId = stripePaymentIntentId;
                invoice.PaymentMethod         = "Stripe";
                await _db.SaveChangesAsync(ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking invoice {InvoiceId} as paid", invoiceId);
            throw;
        }
    }
}
