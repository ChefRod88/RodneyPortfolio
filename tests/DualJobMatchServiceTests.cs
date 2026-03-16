using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using RodneyPortfolio.Models;
using RodneyPortfolio.Services;

namespace RodneyPortfolio.Tests;

public class DualJobMatchServiceTests
{
    private readonly Mock<IResumeContextLoader> _resumeLoaderMock;
    private readonly Mock<IOpenAIClient> _openAiClientMock;
    private readonly Mock<IAnthropicClient> _anthropicClientMock;
    private readonly IConfiguration _configBoth;

    public DualJobMatchServiceTests()
    {
        _resumeLoaderMock = new Mock<IResumeContextLoader>();
        _resumeLoaderMock.Setup(x => x.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("Rodney Chery - Technical Support Analyst.");

        _openAiClientMock    = new Mock<IOpenAIClient>();
        _anthropicClientMock = new Mock<IAnthropicClient>();

        _configBoth = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OpenAI:ApiKey"]    = "openai-key",
                ["OpenAI:Model"]     = "gpt-4o-mini",
                ["Anthropic:ApiKey"] = "sk-ant-key",
                ["Anthropic:Model"]  = "claude-sonnet-4-6"
            })
            .Build();
    }

    private DualJobMatchService BuildService()
    {
        var openAiService = new JobMatchService(
            _resumeLoaderMock.Object,
            _configBoth,
            _openAiClientMock.Object,
            new Mock<ILogger<JobMatchService>>().Object);

        var anthropicService = new AnthropicJobMatchService(
            _resumeLoaderMock.Object,
            _configBoth,
            _anthropicClientMock.Object,
            new Mock<ILogger<AnthropicJobMatchService>>().Object);

        return new DualJobMatchService(
            openAiService,
            anthropicService,
            _resumeLoaderMock.Object,
            new Mock<ILogger<DualJobMatchService>>().Object);
    }

    private static string OpenAiJobJson(int score, string skill, string gap, string point)
    {
        var inner = $"{{\"matchScore\":{score},\"skillsAligned\":[\"{skill}\"],\"gaps\":[\"{gap}\"],\"talkingPoints\":[\"{point}\"]}}";
        return $"{{\"choices\":[{{\"message\":{{\"content\":{System.Text.Json.JsonSerializer.Serialize(inner)}}}}}]}}";
    }

    private static string AnthropicJobJson(int score, string skill, string gap, string point)
    {
        var inner = $"{{\"matchScore\":{score},\"skillsAligned\":[\"{skill}\"],\"gaps\":[\"{gap}\"],\"talkingPoints\":[\"{point}\"]}}";
        return $"{{\"content\":[{{\"type\":\"text\",\"text\":{System.Text.Json.JsonSerializer.Serialize(inner)}}}]}}";
    }

    [Fact]
    public async Task AnalyzeAsync_AveragesScoreAndUnionsLists_WhenBothSucceed()
    {
        _openAiClientMock
            .Setup(x => x.PostChatCompletionsAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    OpenAiJobJson(80, "C#", "Kubernetes", "Enterprise support experience"),
                    Encoding.UTF8, "application/json")
            });

        _anthropicClientMock
            .Setup(x => x.PostMessagesAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    AnthropicJobJson(60, "ASP.NET Core", "Kubernetes", "Career transition story"),
                    Encoding.UTF8, "application/json")
            });

        var service = BuildService();
        var result = await service.AnalyzeAsync("Software Engineer role requiring C# and Kubernetes.");

        Assert.Equal(70, result.MatchScore); // (80+60)/2
        Assert.Contains("C#", result.SkillsAligned);
        Assert.Contains("ASP.NET Core", result.SkillsAligned);
        Assert.Single(result.Gaps); // "Kubernetes" deduplicated
        Assert.Equal("Kubernetes", result.Gaps[0]);
        Assert.Equal(2, result.TalkingPoints.Count);
    }

    [Fact]
    public async Task AnalyzeAsync_ReturnsSingleResult_WhenOpenAIFails()
    {
        _openAiClientMock
            .Setup(x => x.PostChatCompletionsAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));

        _anthropicClientMock
            .Setup(x => x.PostMessagesAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    AnthropicJobJson(75, "ASP.NET Core", "Docker", "Adaptability"),
                    Encoding.UTF8, "application/json")
            });

        var service = BuildService();
        var result = await service.AnalyzeAsync("Backend engineer role.");

        Assert.Equal(75, result.MatchScore);
        Assert.Contains("ASP.NET Core", result.SkillsAligned);
    }

    [Fact]
    public async Task AnalyzeAsync_ReturnsSingleResult_WhenClaudeFails()
    {
        _openAiClientMock
            .Setup(x => x.PostChatCompletionsAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    OpenAiJobJson(85, "C#", "Go", "Canon experience"),
                    Encoding.UTF8, "application/json")
            });

        _anthropicClientMock
            .Setup(x => x.PostMessagesAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var service = BuildService();
        var result = await service.AnalyzeAsync("Backend engineer role.");

        Assert.Equal(85, result.MatchScore);
        Assert.Contains("C#", result.SkillsAligned);
    }

    [Fact]
    public async Task AnalyzeAsync_ReturnsFallback_WhenBothFail()
    {
        _openAiClientMock
            .Setup(x => x.PostChatCompletionsAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));

        _anthropicClientMock
            .Setup(x => x.PostMessagesAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var service = BuildService();
        var result = await service.AnalyzeAsync("Some job description.");

        Assert.Equal(0, result.MatchScore);
        Assert.NotEmpty(result.Gaps);
    }
}
