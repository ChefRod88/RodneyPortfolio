using System.Net;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using RodneyPortfolio.Models;
using RodneyPortfolio.Services;

namespace RodneyPortfolio.Tests;

public class RecaptchaVerificationServiceTests
{
    [Fact]
    public async Task VerifyAsync_ReturnsFailure_WhenSecretIsMissing()
    {
        var service = CreateService(
            new RecaptchaOptions { SecretKey = "" },
            _ => new HttpResponseMessage(HttpStatusCode.OK));

        var result = await service.VerifyAsync("token", "127.0.0.1");

        Assert.False(result.IsSuccess);
        Assert.Contains("missing-config-secret", result.ErrorCodes);
    }

    [Fact]
    public async Task VerifyAsync_ReturnsFailure_WhenTokenIsMissing()
    {
        var service = CreateService(
            new RecaptchaOptions { SecretKey = "secret" },
            _ => new HttpResponseMessage(HttpStatusCode.OK));

        var result = await service.VerifyAsync("", "127.0.0.1");

        Assert.False(result.IsSuccess);
        Assert.Contains("missing-token", result.ErrorCodes);
    }

    [Fact]
    public async Task VerifyAsync_ReturnsFailure_WhenGoogleRejectsToken()
    {
        var service = CreateService(
            new RecaptchaOptions { SecretKey = "secret" },
            _ => JsonResponse("""{"success":false,"error-codes":["invalid-input-response"]}"""));

        var result = await service.VerifyAsync("bad-token", "127.0.0.1");

        Assert.False(result.IsSuccess);
        Assert.Contains("invalid-input-response", result.ErrorCodes);
    }

    [Fact]
    public async Task VerifyAsync_ReturnsFailure_WhenHostnameDoesNotMatch()
    {
        var service = CreateService(
            new RecaptchaOptions { SecretKey = "secret", ExpectedHostname = "www.rodneyachery.com" },
            _ => JsonResponse("""{"success":true,"hostname":"attacker.example"}"""));

        var result = await service.VerifyAsync("valid-token", "127.0.0.1");

        Assert.False(result.IsSuccess);
        Assert.Contains("hostname-mismatch", result.ErrorCodes);
    }

    [Fact]
    public async Task VerifyAsync_ReturnsSuccess_WhenGoogleApprovesAndHostnameMatches()
    {
        var service = CreateService(
            new RecaptchaOptions { SecretKey = "secret", ExpectedHostname = "www.rodneyachery.com" },
            _ => JsonResponse("""{"success":true,"hostname":"www.rodneyachery.com"}"""));

        var result = await service.VerifyAsync("valid-token", "127.0.0.1");

        Assert.True(result.IsSuccess);
        Assert.Empty(result.ErrorCodes);
    }

    private static RecaptchaVerificationService CreateService(
        RecaptchaOptions options,
        Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        var client = new HttpClient(new StubHttpMessageHandler(responder));
        return new RecaptchaVerificationService(
            client,
            Options.Create(options),
            NullLogger<RecaptchaVerificationService>.Instance);
    }

    private static HttpResponseMessage JsonResponse(string json) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

        public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            _responder = responder;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(_responder(request));
    }
}
