using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using RodneyPortfolio.Services;

namespace RodneyPortfolio.Tests;

public class OpenAIChatServiceTests
{
    private readonly Mock<IResumeContextLoader> _resumeLoaderMock;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<ILogger<OpenAIChatService>> _loggerMock;

    public OpenAIChatServiceTests()
    {
        _resumeLoaderMock = new Mock<IResumeContextLoader>();
        _resumeLoaderMock.Setup(x => x.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("Rodney Chery - Technical Support Analyst. Experience at Canon.");

        _httpClientFactoryMock = new Mock<IHttpClientFactory>();

        _loggerMock = new Mock<ILogger<OpenAIChatService>>();
    }

    [Fact]
    public async Task GetReplyAsync_ReturnsDemoResponse_WhenApiKeyIsEmpty()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OpenAI:ApiKey"] = "",
                ["OpenAI:UseDemoMode"] = "true"
            })
            .Build();

        var service = new OpenAIChatService(
            _resumeLoaderMock.Object,
            config,
            _httpClientFactoryMock.Object,
            _loggerMock.Object);

        var (reply, source) = await service.GetReplyAsync("What's Rodney's experience?");

        Assert.NotNull(reply);
        Assert.Equal("demo", source);
        Assert.Contains("Canon", reply);
        Assert.Contains("Technical Support", reply);
    }

    [Fact]
    public async Task GetReplyAsync_ReturnsDemoResponse_WhenAskedAboutEducation()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OpenAI:ApiKey"] = "",
                ["OpenAI:UseDemoMode"] = "true"
            })
            .Build();

        var service = new OpenAIChatService(
            _resumeLoaderMock.Object,
            config,
            _httpClientFactoryMock.Object,
            _loggerMock.Object);

        var (reply, source) = await service.GetReplyAsync("What's his education?");

        Assert.NotNull(reply);
        Assert.Equal("demo", source);
        Assert.Contains("WGU", reply);
        Assert.Contains("Software Engineering", reply);
    }

    [Fact]
    public async Task GetReplyAsync_ReturnsDefaultDemoResponse_WhenQuestionNotMatched()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OpenAI:ApiKey"] = "",
                ["OpenAI:UseDemoMode"] = "true"
            })
            .Build();

        var service = new OpenAIChatService(
            _resumeLoaderMock.Object,
            config,
            _httpClientFactoryMock.Object,
            _loggerMock.Object);

        var (reply, source) = await service.GetReplyAsync("xyz random question 123");

        Assert.NotNull(reply);
        Assert.Equal("demo", source);
        Assert.Contains("background", reply);
        Assert.Contains("experience", reply);
    }
}
