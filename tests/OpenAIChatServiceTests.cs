using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using RodneyPortfolio.Services;

namespace RodneyPortfolio.Tests;

public class OpenAIChatServiceTests
{
    private readonly Mock<IResumeContextLoader> _resumeLoaderMock;
    private readonly Mock<IOpenAIClient> _openAiClientMock;
    private readonly Mock<ILogger<OpenAIChatService>> _loggerMock;

    public OpenAIChatServiceTests()
    {
        _resumeLoaderMock = new Mock<IResumeContextLoader>();
        _resumeLoaderMock.Setup(x => x.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("Rodney Chery - Technical Support Analyst. Experience at Canon.");

        _openAiClientMock = new Mock<IOpenAIClient>();
        _loggerMock = new Mock<ILogger<OpenAIChatService>>();
    }

    [Fact]
    public async Task GetReplyAsync_ReturnsConfigError_WhenApiKeyIsEmpty()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OpenAI:ApiKey"] = ""
            })
            .Build();

        var service = new OpenAIChatService(
            _resumeLoaderMock.Object,
            config,
            _openAiClientMock.Object,
            _loggerMock.Object);

        var reply = await service.GetReplyAsync("What's Rodney's experience?");

        Assert.NotNull(reply);
        Assert.Contains("not configured", reply);
        _resumeLoaderMock.Verify(x => x.LoadAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
