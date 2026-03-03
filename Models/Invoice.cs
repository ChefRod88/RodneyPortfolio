namespace RodneyPortfolio.Models;

public class ClientRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Tier { get; set; } = "Starter";
    public decimal MonthlyRate { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public bool Active { get; set; } = true;
}

public class Invoice
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ClientId { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string ClientEmail { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTimeOffset IssuedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset DueAt { get; set; }
    public DateTimeOffset? PaidAt { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Unpaid;
    public string? StripePaymentIntentId { get; set; }
    public string? StripeCheckoutSessionId { get; set; }

    public string? PaymentMethod { get; set; }

    public string? InvoiceNumber { get; set; }
}

public enum InvoiceStatus
{
    Unpaid,
    Paid,
    Overdue,
    Cancelled,

    PendingCashApp
}