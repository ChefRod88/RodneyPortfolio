namespace RodneyPortfolio.Services;

public interface IRecaptchaVerificationService
{
    Task<RecaptchaVerificationResult> VerifyAsync(string token, string? remoteIp, CancellationToken cancellationToken = default);
}

public sealed record RecaptchaVerificationResult(bool IsSuccess, string[] ErrorCodes);
