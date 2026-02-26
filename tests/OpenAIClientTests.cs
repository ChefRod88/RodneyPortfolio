using System.Net;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using RodneyPortfolio.Services;

namespace RodneyPortfolio.Tests;

public class OpenAIClientTests
{
    [Fact]
    public async Task PostChatCompletionsAsync_UsesBearerApiKeyAndEndpoint()
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
                Content = new StringContent("{\"choices\":[]}")
            });

        var httpClient = new HttpClient(handler.Object);
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OpenAI:ApiKey"] = "test-key"
            })
            .Build();

        var client = new OpenAIClient(config, httpClientFactoryMock.Object);

        await client.PostChatCompletionsAsync(new { model = "gpt-4o-mini", messages = new[] { new { role = "user", content = "hi" } } });

        Assert.NotNull(capturedRequest);
        Assert.Equal(HttpMethod.Post, capturedRequest!.Method);
        Assert.Equal("https://api.openai.com/v1/chat/completions", capturedRequest.RequestUri?.ToString());
        Assert.NotNull(capturedRequest.Headers.Authorization);
        Assert.Equal("Bearer", capturedRequest.Headers.Authorization!.Scheme);
        Assert.Equal("test-key", capturedRequest.Headers.Authorization.Parameter);
    }

    [Fact]
    public async Task PostChatCompletionsAsync_ThrowsWhenApiKeyMissing()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        var client = new OpenAIClient(config, httpClientFactoryMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            client.PostChatCompletionsAsync(new { model = "gpt-4o-mini" }));
    }
}
