using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using RodneyPortfolio.Models;

namespace RodneyPortfolio.Services;

public sealed class RecaptchaVerificationService : IRecaptchaVerificationService
{
    private const string VerifyEndpoint = "https://www.google.com/recaptcha/api/siteverify";

    private readonly HttpClient _httpClient;
    private readonly RecaptchaOptions _options;
    private readonly ILogger<RecaptchaVerificationService> _logger;

    public RecaptchaVerificationService(
        HttpClient httpClient,
        IOptions<RecaptchaOptions> options,
        ILogger<RecaptchaVerificationService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<RecaptchaVerificationResult> VerifyAsync(string token, string? remoteIp, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.SecretKey))
        {
            _logger.LogError("Recaptcha secret key is not configured.");
            return new RecaptchaVerificationResult(false, ["missing-config-secret"]);
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            return new RecaptchaVerificationResult(false, ["missing-token"]);
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
            _logger.LogWarning("Recaptcha verify endpoint returned status code {StatusCode}", response.StatusCode);
            return new RecaptchaVerificationResult(false, ["verify-http-failed"]);
        }

        var payload = await response.Content.ReadFromJsonAsync<RecaptchaVerifyApiResponse>(cancellationToken: cancellationToken);
        if (payload is null)
        {
            _logger.LogWarning("Recaptcha verify endpoint returned an empty payload.");
            return new RecaptchaVerificationResult(false, ["verify-empty-payload"]);
        }

        var errors = payload.ErrorCodes ?? [];
        if (!payload.Success)
        {
            return new RecaptchaVerificationResult(false, errors);
        }

        if (!string.IsNullOrWhiteSpace(_options.ExpectedHostname) &&
            !string.Equals(_options.ExpectedHostname, payload.Hostname, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(
                "Recaptcha hostname mismatch. Expected {ExpectedHostname}, received {ActualHostname}",
                _options.ExpectedHostname,
                payload.Hostname);
            return new RecaptchaVerificationResult(false, ["hostname-mismatch"]);
        }

        return new RecaptchaVerificationResult(true, errors);
    }

    private sealed class RecaptchaVerifyApiResponse
    {
        public bool Success { get; set; }
        public string Hostname { get; set; } = string.Empty;
        [JsonPropertyName("error-codes")]
        public string[]? ErrorCodes { get; set; }
    }
}
