using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using RodneyPortfolio.Services;

namespace RodneyPortfolio.Tests;

public class DualAIChatServiceTests
{
    private readonly Mock<IResumeContextLoader> _resumeLoaderMock;
    private readonly Mock<IOpenAIClient> _openAiClientMock;
    private readonly Mock<IAnthropicClient> _anthropicClientMock;
    private readonly IConfiguration _configBoth;

    public DualAIChatServiceTests()
    {
        _resumeLoaderMock = new Mock<IResumeContextLoader>();
        _resumeLoaderMock.Setup(x => x.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("Rodney Chery - Technical Support Analyst.");

        _openAiClientMock = new Mock<IOpenAIClient>();
        _anthropicClientMock = new Mock<IAnthropicClient>();

        _configBoth = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OpenAI:ApiKey"]     = "openai-key",
                ["OpenAI:Model"]      = "gpt-4o-mini",
                ["Anthropic:ApiKey"]  = "sk-ant-key",
                ["Anthropic:Model"]   = "claude-sonnet-4-6"
            })
            .Build();
    }

    private DualAIChatService BuildService()
    {
        var openAiService = new OpenAIChatService(
            _resumeLoaderMock.Object,
            _configBoth,
            _openAiClientMock.Object,
            new Mock<ILogger<OpenAIChatService>>().Object);

        var anthropicService = new AnthropicChatService(
            _resumeLoaderMock.Object,
            _configBoth,
            _anthropicClientMock.Object,
            new Mock<ILogger<AnthropicChatService>>().Object);

        return new DualAIChatService(
            openAiService,
            anthropicService,
            _resumeLoaderMock.Object,
            new Mock<ILogger<DualAIChatService>>().Object);
    }

    [Fact]
    public async Task GetReplyAsync_MergesBothReplies_WhenBothSucceed()
    {
        _openAiClientMock
            .Setup(x => x.PostChatCompletionsAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"choices":[{"message":{"content":"OpenAI says hello."}}]}""",
                    Encoding.UTF8, "application/json")
            });

        _anthropicClientMock
            .Setup(x => x.PostMessagesAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"content":[{"type":"text","text":"Claude says hello."}]}""",
                    Encoding.UTF8, "application/json")
            });

        var service = BuildService();
        var reply = await service.GetReplyAsync("Hi");

        Assert.Contains("OpenAI says hello.", reply);
        Assert.Contains("Claude says hello.", reply);
        Assert.Contains("---", reply);
    }

    [Fact]
    public async Task GetReplyAsync_ReturnsClaude_WhenOpenAIFails()
    {
        _openAiClientMock
            .Setup(x => x.PostChatCompletionsAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));

        _anthropicClientMock
            .Setup(x => x.PostMessagesAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"content":[{"type":"text","text":"Only Claude responded."}]}""",
                    Encoding.UTF8, "application/json")
            });

        var service = BuildService();
        var reply = await service.GetReplyAsync("Hi");

        Assert.Equal("Only Claude responded.", reply);
    }

    [Fact]
    public async Task GetReplyAsync_ReturnsOpenAI_WhenClaudeFails()
    {
        _openAiClientMock
            .Setup(x => x.PostChatCompletionsAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"choices":[{"message":{"content":"Only OpenAI responded."}}]}""",
                    Encoding.UTF8, "application/json")
            });

        _anthropicClientMock
            .Setup(x => x.PostMessagesAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var service = BuildService();
        var reply = await service.GetReplyAsync("Hi");

        Assert.Equal("Only OpenAI responded.", reply);
    }

    [Fact]
    public async Task GetReplyAsync_ReturnsDemoFallback_WhenBothFail()
    {
        _openAiClientMock
            .Setup(x => x.PostChatCompletionsAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));

        _anthropicClientMock
            .Setup(x => x.PostMessagesAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var service = BuildService();
        var reply = await service.GetReplyAsync("Where does Rodney work?");

        Assert.NotEmpty(reply);
        Assert.Contains("Canon", reply);
    }
}
