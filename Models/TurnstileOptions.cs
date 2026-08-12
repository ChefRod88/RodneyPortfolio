namespace RodneyPortfolio.Models;

public sealed class TurnstileOptions
{
    public const string SectionName = "Turnstile";

    public string SiteKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string? ExpectedHostname { get; set; }
}
