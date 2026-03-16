using System.Net;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using RodneyPortfolio.Services;

namespace RodneyPortfolio.Tests;

public class AnthropicClientTests
{
    [Fact]
    public async Task PostMessagesAsync_UsesXApiKeyHeaderAndCorrectEndpoint()
    {
        HttpRequestMessage? capturedRequest = null;
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"content\":[]}")
            });

        var httpClient = new HttpClient(handler.Object);
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Anthropic:ApiKey"] = "sk-ant-test-key"
            })
            .Build();

        var client = new AnthropicClient(config, httpClientFactoryMock.Object);

        await client.PostMessagesAsync(new { model = "claude-sonnet-4-6", max_tokens = 512, messages = new[] { new { role = "user", content = "hi" } } });

        Assert.NotNull(capturedRequest);
        Assert.Equal(HttpMethod.Post, capturedRequest!.Method);
        Assert.Equal("https://api.anthropic.com/v1/messages", capturedRequest.RequestUri?.ToString());
        Assert.True(capturedRequest.Headers.TryGetValues("x-api-key", out var keyValues));
        Assert.Equal("sk-ant-test-key", keyValues!.First());
        Assert.True(capturedRequest.Headers.TryGetValues("anthropic-version", out var versionValues));
        Assert.Equal("2023-06-01", versionValues!.First());
    }

    [Fact]
    public async Task PostMessagesAsync_ThrowsWhenApiKeyMissing()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        var client = new AnthropicClient(config, httpClientFactoryMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            client.PostMessagesAsync(new { model = "claude-sonnet-4-6" }));
    }
}
