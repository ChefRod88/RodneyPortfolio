using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using RodneyPortfolio.Models;

namespace RodneyPortfolio.Services;

public sealed class TurnstileVerificationService : ITurnstileVerificationService
{
    private const string VerifyEndpoint = "https://challenges.cloudflare.com/turnstile/v0/siteverify";

    private readonly HttpClient _httpClient;
    private readonly TurnstileOptions _options;
    private readonly ILogger<TurnstileVerificationService> _logger;

    public TurnstileVerificationService(
        HttpClient httpClient,
        IOptions<TurnstileOptions> options,
        ILogger<TurnstileVerificationService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<TurnstileVerificationResult> VerifyAsync(string token, string? remoteIp, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.SecretKey))
        {
            _logger.LogError("Turnstile secret key is not configured.");
            return new TurnstileVerificationResult(false, ["missing-config-secret"]);
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            return new TurnstileVerificationResult(false, ["missing-token"]);
        }

        using var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["secret"] = _options.SecretKey,
            ["response"] = token,
            ["remoteip"] = remoteIp ?? string.Empty
        });

        using var response = await _httpClient.PostAsync(VerifyEndpoint, form, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Turnstile verify endpoint returned status code {StatusCode}", response.StatusCode);
            return new TurnstileVerificationResult(false, ["verify-http-failed"]);
        }

        var payload = await response.Content.ReadFromJsonAsync<TurnstileVerifyApiResponse>(cancellationToken: cancellationToken);
        if (payload is null)
        {
            _logger.LogWarning("Turnstile verify endpoint returned an empty payload.");
            return new TurnstileVerificationResult(false, ["verify-empty-payload"]);
        }

        var errors = payload.ErrorCodes ?? [];
        if (!payload.Success)
        {
            return new TurnstileVerificationResult(false, errors);
        }

        if (!string.IsNullOrWhiteSpace(_options.ExpectedHostname) &&
            !string.Equals(_options.ExpectedHostname, payload.Hostname, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(
                "Turnstile hostname mismatch. Expected {ExpectedHostname}, received {ActualHostname}",
                _options.ExpectedHostname,
                payload.Hostname);
            return new TurnstileVerificationResult(false, ["hostname-mismatch"]);
        }

        return new TurnstileVerificationResult(true, errors);
    }

    private sealed class TurnstileVerifyApiResponse
    {
        public bool Success { get; set; }
        public string Hostname { get; set; } = string.Empty;
        [JsonPropertyName("error-codes")]
        public string[]? ErrorCodes { get; set; }
    }
}
