namespace RodneyPortfolio.Services;

public interface ITurnstileVerificationService
{
    Task<TurnstileVerificationResult> VerifyAsync(string token, string? remoteIp, CancellationToken cancellationToken = default);
}

public sealed record TurnstileVerificationResult(bool IsSuccess, string[] ErrorCodes);
