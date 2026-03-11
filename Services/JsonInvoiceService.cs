using System.Text.Json;
using RodneyPortfolio.Models;

namespace RodneyPortfolio.Services;

public class JsonInvoiceService : IInvoiceService
{
    private readonly string _dataDir;
    private readonly ILogger<JsonInvoiceService> _logger;
    private static readonly JsonSerializerOptions _json = new() { WriteIndented = true };

    public JsonInvoiceService(IWebHostEnvironment env, ILogger<JsonInvoiceService> logger)
    {
        _dataDir = Path.Combine(env.ContentRootPath, "Data");
        Directory.CreateDirectory(_dataDir);
        _logger = logger;
    }

    private string ClientsPath => Path.Combine(_dataDir, "Clients.json");
    private string InvoicesPath => Path.Combine(_dataDir, "Invoices.json");

    private async Task<List<ClientRecord>> LoadClientsAsync()
    {
        if (!File.Exists(ClientsPath)) return new();
        return JsonSerializer.Deserialize<List<ClientRecord>>(await File.ReadAllTextAsync(ClientsPath)) ?? new();
    }

    private async Task SaveClientsAsync(List<ClientRecord> clients)
        => await File.WriteAllTextAsync(ClientsPath, JsonSerializer.Serialize(clients, _json));

    public async Task<ClientRecord?> GetClientByEmailAsync(string email, CancellationToken ct = default)
        => (await LoadClientsAsync()).FirstOrDefault(c => c.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

    public async Task<List<ClientRecord>> GetAllClientsAsync(CancellationToken ct = default)
        => await LoadClientsAsync();

    public async Task SaveClientAsync(ClientRecord client, CancellationToken ct = default)
    {
        var clients = await LoadClientsAsync();
        var idx = clients.FindIndex(c => c.Id == client.Id);
        if (idx >= 0) clients[idx] = client; else clients.Add(client);
        await SaveClientsAsync(clients);
    }

    private async Task<List<Invoice>> LoadInvoicesAsync()
    {
        if (!File.Exists(InvoicesPath)) return new();
        return JsonSerializer.Deserialize<List<Invoice>>(await File.ReadAllTextAsync(InvoicesPath)) ?? new();
    }

    private async Task SaveInvoicesAsync(List<Invoice> invoices)
        => await File.WriteAllTextAsync(InvoicesPath, JsonSerializer.Serialize(invoices, _json));

    public async Task<List<Invoice>> GetInvoicesForClientAsync(string clientId, CancellationToken ct = default)
        => (await LoadInvoicesAsync()).Where(i => i.ClientId == clientId).OrderByDescending(i => i.IssuedAt).ToList();

    public async Task<Invoice?> GetInvoiceByIdAsync(string invoiceId, CancellationToken ct = default)
        => (await LoadInvoicesAsync()).FirstOrDefault(i => i.Id == invoiceId);

    public async Task<List<Invoice>> GetAllInvoicesAsync(CancellationToken ct = default)
        => (await LoadInvoicesAsync()).OrderByDescending(i => i.IssuedAt).ToList();

    public async Task SaveInvoiceAsync(Invoice invoice, CancellationToken ct = default)
    {
        var invoices = await LoadInvoicesAsync();
        var idx = invoices.FindIndex(i => i.Id == invoice.Id);
        if (idx >= 0) invoices[idx] = invoice; else invoices.Add(invoice);
        await SaveInvoicesAsync(invoices);
    }

    public async Task UpdateInvoiceAsync(Invoice invoice, CancellationToken ct = default)
        => await SaveInvoiceAsync(invoice, ct);

    public async Task MarkInvoicePaidAsync(string invoiceId, string stripePaymentIntentId, CancellationToken ct = default)
    {
        var invoices = await LoadInvoicesAsync();
        var inv = invoices.FirstOrDefault(i => i.Id == invoiceId);
        if (inv is null) return;
        inv.Status                = InvoiceStatus.Paid;
        inv.PaidAt                = DateTimeOffset.UtcNow;
        inv.StripePaymentIntentId = stripePaymentIntentId;
        inv.PaymentMethod         = "Stripe";
        await SaveInvoicesAsync(invoices);
    }
}