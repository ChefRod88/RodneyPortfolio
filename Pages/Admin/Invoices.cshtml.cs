using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RodneyPortfolio.Models;
using RodneyPortfolio.Services;
using System.ComponentModel.DataAnnotations;

namespace RodneyPortfolio.Pages.Admin;

public class InvoicesModel : PageModel
{
    private readonly IInvoiceService _invoiceService;
    private readonly IClientPortalService _portalService;

    public InvoicesModel(IInvoiceService invoiceService, IClientPortalService portalService)
    {
        _invoiceService = invoiceService;
        _portalService = portalService;
    }

    public List<Invoice> Invoices { get; private set; } = new();
    public List<ClientAccount> Clients { get; private set; } = new();
    public string? StatusMessage { get; private set; }

    [BindProperty]
    public CreateInvoiceInput Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        if (!AdminGuard.IsAdminAuthenticated(HttpContext))
            return RedirectToPage("/Admin/AdminLogin");

        await LoadDataAsync(ct);
        return Page();
    }

    public async Task<IActionResult> OnPostCreateAsync(CancellationToken ct)
    {
        if (!AdminGuard.IsAdminAuthenticated(HttpContext))
            return RedirectToPage("/Admin/AdminLogin");

        if (!ModelState.IsValid)
        {
            await LoadDataAsync(ct);
            return Page();
        }

        var client = await _portalService.GetByEmailAsync(Input.ClientEmail.Trim().ToLowerInvariant(), ct);
        if (client is null)
        {
            ModelState.AddModelError("Input.ClientEmail", "No registered client found with that email.");
            await LoadDataAsync(ct);
            return Page();
        }

        var invoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}";

        var invoice = new Invoice
        {
            ClientId      = client.Id,
            ClientName    = client.FullName,
            ClientEmail   = client.Email,
            Amount        = Input.Amount,
            Description   = Input.Description.Trim(),
            IssuedAt      = DateTimeOffset.UtcNow,
            DueAt         = new DateTimeOffset(Input.DueDate, TimeOnly.MinValue, TimeSpan.Zero),
            Status        = InvoiceStatus.Unpaid,
            InvoiceNumber = invoiceNumber
        };

        await _invoiceService.SaveInvoiceAsync(invoice, ct);
        StatusMessage = $"Invoice {invoiceNumber} created for {client.FullName}.";

        await LoadDataAsync(ct);
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(string id, CancellationToken ct)
    {
        if (!AdminGuard.IsAdminAuthenticated(HttpContext))
            return RedirectToPage("/Admin/AdminLogin");

        var invoices = await _invoiceService.GetAllInvoicesAsync(ct);
        var invoice = invoices.FirstOrDefault(i => i.Id == id);
        if (invoice is not null)
        {
            invoice.Status = InvoiceStatus.Cancelled;
            await _invoiceService.UpdateInvoiceAsync(invoice, ct);
        }

        return RedirectToPage("/Admin/Invoices");
    }

    private async Task LoadDataAsync(CancellationToken ct)
    {
        Invoices = (await _invoiceService.GetAllInvoicesAsync(ct))
            .OrderByDescending(i => i.IssuedAt)
            .ToList();
        Clients = await _portalService.GetAllAccountsAsync(ct);
    }
}

public class CreateInvoiceInput
{
    [Required, EmailAddress]
    public string ClientEmail { get; set; } = string.Empty;

    [Required, StringLength(200, MinimumLength = 3)]
    public string Description { get; set; } = string.Empty;

    [Required, Range(1, 100000)]
    public decimal Amount { get; set; }

    [Required]
    public DateOnly DueDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14));
}
