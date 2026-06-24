using RodneyPortfolio.Models;

namespace RodneyPortfolio.Services;

/// <summary>
/// Interface for sending the signed software agreement email.
/// </summary>
public interface IAgreementEmailService
{
    Task SendAsync(AgreementSubmissionInput input, CancellationToken ct = default);
}
