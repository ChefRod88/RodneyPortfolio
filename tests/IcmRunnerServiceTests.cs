using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using RodneyPortfolio.Services;
using Xunit;

namespace RodneyPortfolio.Tests;

public class IcmRunnerServiceTests
{
    private readonly Mock<IOpenAIClient> _openAiClientMock;
    private readonly Mock<ILogger<IcmRunnerService>> _loggerMock;
    private readonly IConfiguration _config;

    public IcmRunnerServiceTests()
    {
        _openAiClientMock = new Mock<IOpenAIClient>();
        _loggerMock = new Mock<ILogger<IcmRunnerService>>();

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OpenAI:ApiKey"] = "test-key",
                ["OpenAI:Model"] = "gpt-4o-mini"
            })
            .Build();
    }

    [Fact]
    public async Task RunPipelineAsync_ExecutesAllThreeStages_AndParsesFinalOutput()
    {
        // Stage 1 output mock
        var stage1Response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{"choices":[{"message":{"content":"# Stage 1: Extracted Skills\n- C#\n- ASP.NET Core\n- Support"}}]}""",
                Encoding.UTF8, "application/json")
        };

        // Stage 2 output mock
        var stage2Response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{"choices":[{"message":{"content":"# Stage 2: Comparison\n- Skills Aligned: C#, ASP.NET Core\n- Gaps: None"}}]}""",
                Encoding.UTF8, "application/json")
        };

        // Stage 3 output mock with JSON block at the bottom
        var stage3Response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """
                {"choices":[{"message":{"content":"# Stage 3: Interview Prep\n\n```json\n{\n  \"matchScore\": 95,\n  \"skillsAligned\": [\"C#\", \"ASP.NET Core\"],\n  \"gaps\": [\"Kubernetes\"],\n  \"talkingPoints\": [\"Highlight custom projects\"]\n}\n```"}}]}
                """,
                Encoding.UTF8, "application/json")
        };

        // Setup client mock to return different answers sequentially
        var callCount = 0;
        _openAiClientMock
            .Setup(x => x.PostChatCompletionsAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                callCount++;
                return callCount switch
                {
                    1 => stage1Response,
                    2 => stage2Response,
                    3 => stage3Response,
                    _ => throw new InvalidOperationException("Unexpected mock call count")
                };
            });

        var service = new IcmRunnerService(_config, _openAiClientMock.Object, _loggerMock.Object);

        // Run pipeline
        var result = await service.RunPipelineAsync("We need a C# developer who builds APIs.");

        // Assert
        Assert.Equal(3, callCount);
        Assert.NotNull(result);
        Assert.Equal(95, result.FinalAnalysis.MatchScore);
        Assert.Contains("C#", result.FinalAnalysis.SkillsAligned);
        Assert.Contains("Kubernetes", result.FinalAnalysis.Gaps);
        Assert.Contains("# Stage 1", result.Stage1Output);
        Assert.Contains("# Stage 2", result.Stage2Output);
        Assert.Contains("# Stage 3", result.Stage3Output);

        // Verify files were written
        var workspaceRoot = Path.Combine(Directory.GetCurrentDirectory(), "IcmWorkspace");
        Assert.True(File.Exists(Path.Combine(workspaceRoot, "01_skill_extraction", "working", "extracted_requirements.md")));
        Assert.True(File.Exists(Path.Combine(workspaceRoot, "02_resume_comparison", "working", "comparison_results.md")));
        Assert.True(File.Exists(Path.Combine(workspaceRoot, "03_interview_preparation", "working", "interview_guide.md")));
    }
}
