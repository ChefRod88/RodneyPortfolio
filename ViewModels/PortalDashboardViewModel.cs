using RodneyPortfolio.Models;

namespace RodneyPortfolio.ViewModels;

public class PortalDashboardViewModel
{
    public ClientAccount Account { get; set; } = new();
    public List<Invoice> OpenInvoices { get; set; } = new();
    public List<Invoice> PaidInvoices { get; set; } = new();
    public decimal TotalDue => OpenInvoices.Sum(i => i.Amount);
    public decimal TotalPaid => PaidInvoices.Sum(i => i.Amount);
    public int UnpaidCount => OpenInvoices.Count;
    public string StripePublishableKey { get; set; } = string.Empty;
    public string? StatusMessage { get; set; }
    public SupportMessageInput SupportMsg { get; set; } = new();
}
