namespace RodneyPortfolio.Models;

public record CreatePaymentIntentRequest(string InvoiceId);
public record ConfirmPaymentRequest(string InvoiceId, string PaymentIntentId);
public record CashAppPendingRequest(string InvoiceId);
