namespace RodneyPortfolio.Models;

/// <summary>
/// Model representing the client information and the Base64-encoded PDF contract signature data.
/// </summary>
public class AgreementSubmissionInput
{
    public string ClientName { get; set; } = string.Empty;
    public string ClientCompany { get; set; } = string.Empty;
    public string ClientEmail { get; set; } = string.Empty;
    public string ClientIp { get; set; } = string.Empty;
    public string PdfBase64 { get; set; } = string.Empty;
}
