using RodneyPortfolio.Models;

namespace RodneyPortfolio.Services;

/// <summary>
/// Runs OpenAI and Anthropic job-match analysis in parallel and merges results.
/// Average score, union of all string arrays with deduplication.
/// Registered as IJobMatchService so ChatController is unaware of the duality.
/// </summary>
public class DualJobMatchService : IJobMatchService
{
    private readonly JobMatchService _openAi;
    private readonly AnthropicJobMatchService _anthropic;
    private readonly IResumeContextLoader _resumeLoader;
    private readonly ILogger<DualJobMatchService> _logger;

    public DualJobMatchService(
        JobMatchService openAi,
        AnthropicJobMatchService anthropic,
        IResumeContextLoader resumeLoader,
        ILogger<DualJobMatchService> logger)
    {
        _openAi = openAi;
        _anthropic = anthropic;
        _resumeLoader = resumeLoader;
        _logger = logger;
    }

    public async Task<JobMatchResponse> AnalyzeAsync(string jobDescription, CancellationToken cancellationToken = default)
    {
        var resumeContext = await _resumeLoader.LoadAsync(cancellationToken);

        var openAiTask  = _openAi.TryAnalyzeAsync(jobDescription, resumeContext, cancellationToken);
        var claudeTask  = _anthropic.TryAnalyzeAsync(jobDescription, resumeContext, cancellationToken);

        await Task.WhenAll(openAiTask, claudeTask);

        var openAiResult = openAiTask.Result;
        var claudeResult = claudeTask.Result;

        if (openAiResult == null && claudeResult == null)
        {
            _logger.LogWarning("Both OpenAI and Anthropic failed for job match. Returning fallback.");
            return FallbackResponse();
        }

        if (openAiResult == null) return claudeResult!;
        if (claudeResult == null) return openAiResult;

        return new JobMatchResponse
        {
            MatchScore    = (openAiResult.MatchScore + claudeResult.MatchScore) / 2,
            SkillsAligned = Union(openAiResult.SkillsAligned, claudeResult.SkillsAligned),
            Gaps          = Union(openAiResult.Gaps, claudeResult.Gaps),
            TalkingPoints = Union(openAiResult.TalkingPoints, claudeResult.TalkingPoints)
        };
    }

    private static List<string> Union(List<string> a, List<string> b) =>
        a.Concat(b)
         .Where(s => !string.IsNullOrWhiteSpace(s))
         .Distinct(StringComparer.OrdinalIgnoreCase)
         .ToList();

    private static JobMatchResponse FallbackResponse() => new()
    {
        MatchScore    = 0,
        SkillsAligned = new List<string>(),
        Gaps          = new List<string> { "Both AI providers are unavailable. Please try again later." },
        TalkingPoints = new List<string>()
    };
}
