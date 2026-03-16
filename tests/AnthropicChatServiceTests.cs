using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using RodneyPortfolio.Services;

namespace RodneyPortfolio.Tests;

public class AnthropicChatServiceTests
{
    private readonly Mock<IResumeContextLoader> _resumeLoaderMock;
    private readonly Mock<IAnthropicClient> _anthropicClientMock;
    private readonly Mock<ILogger<AnthropicChatService>> _loggerMock;

    public AnthropicChatServiceTests()
    {
        _resumeLoaderMock = new Mock<IResumeContextLoader>();
        _resumeLoaderMock.Setup(x => x.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("Rodney Chery - Technical Support Analyst. Experience at Canon.");

        _anthropicClientMock = new Mock<IAnthropicClient>();
        _loggerMock = new Mock<ILogger<AnthropicChatService>>();
    }

    [Fact]
    public async Task GetReplyAsync_ReturnsConfigError_WhenApiKeyIsEmpty()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Anthropic:ApiKey"] = ""
            })
            .Build();

        var service = new AnthropicChatService(
            _resumeLoaderMock.Object,
            config,
            _anthropicClientMock.Object,
            _loggerMock.Object);

        var reply = await service.GetReplyAsync("What's Rodney's experience?");

        Assert.NotNull(reply);
        Assert.Contains("not configured", reply);
        _resumeLoaderMock.Verify(x => x.LoadAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task TryGetReplyAsync_ReturnsNull_WhenApiCallFails()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Anthropic:ApiKey"] = "sk-ant-test"
            })
            .Build();

        _anthropicClientMock
            .Setup(x => x.PostMessagesAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var service = new AnthropicChatService(
            _resumeLoaderMock.Object,
            config,
            _anthropicClientMock.Object,
            _loggerMock.Object);

        var result = await service.TryGetReplyAsync("Hello", null, "resume context", CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task TryGetReplyAsync_ReturnsContent_WhenApiSucceeds()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Anthropic:ApiKey"] = "sk-ant-test"
            })
            .Build();

        var anthropicJson = """{"content":[{"type":"text","text":"Rodney is great!"}]}""";
        _anthropicClientMock
            .Setup(x => x.PostMessagesAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(anthropicJson, Encoding.UTF8, "application/json")
            });

        var service = new AnthropicChatService(
            _resumeLoaderMock.Object,
            config,
            _anthropicClientMock.Object,
            _loggerMock.Object);

        var result = await service.TryGetReplyAsync("Tell me about Rodney", null, "resume context", CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Rodney is great!", result);
    }
}
