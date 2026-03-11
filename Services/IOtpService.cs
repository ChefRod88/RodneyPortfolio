namespace RodneyPortfolio.Services;

/// <summary>
/// Generates and validates one-time passcodes for email verification.
/// </summary>
public interface IOtpService
{
    Task<string> GenerateOtpAsync(string email, string purpose, CancellationToken ct = default);
    Task<bool> ValidateOtpAsync(string email, string code, string purpose, CancellationToken ct = default);
}
