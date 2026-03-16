using System.Net.Http.Json;
using System.Text.Json;
using RodneyPortfolio.Models;

namespace RodneyPortfolio.Services;

/// <summary>
/// Analyzes job descriptions against Rodney's resume using Anthropic's Claude API.
/// </summary>
public class AnthropicJobMatchService : IJobMatchService
{
    private readonly IResumeContextLoader _resumeLoader;
    private readonly IConfiguration _config;
    private readonly IAnthropicClient _anthropicClient;
    private readonly ILogger<AnthropicJobMatchService> _logger;

    public AnthropicJobMatchService(
        IResumeContextLoader resumeLoader,
        IConfiguration config,
        IAnthropicClient anthropicClient,
        ILogger<AnthropicJobMatchService> logger)
    {
        _resumeLoader = resumeLoader;
        _config = config;
        _anthropicClient = anthropicClient;
        _logger = logger;
    }

    public async Task<JobMatchResponse> AnalyzeAsync(string jobDescription, CancellationToken cancellationToken = default)
    {
        var apiKey = _config["Anthropic:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("Anthropic API key not configured");
            return new JobMatchResponse
            {
                MatchScore = 0,
                SkillsAligned = new List<string>(),
                Gaps = new List<string> { "Chatbot is not configured." },
                TalkingPoints = new List<string>()
            };
        }

        var resumeContext = await _resumeLoader.LoadAsync(cancellationToken);
        return await TryAnalyzeAsync(jobDescription, resumeContext, cancellationToken) ?? FallbackResponse();
    }

    /// <summary>
    /// Calls Anthropic for job match analysis and returns the result, or null on any API failure.
    /// Used by DualJobMatchService to run both providers in parallel.
    /// </summary>
    public async Task<JobMatchResponse?> TryAnalyzeAsync(string jobDescription, string resumeContext, CancellationToken cancellationToken)
    {
        var prompt = $@"Compare this job description to Rodney Chery's resume. Return a JSON object with exactly these keys:
- matchScore: integer 0-100
- skillsAligned: array of strings (skills/requirements from the job that Rodney has)
- gaps: array of strings (job requirements Rodney lacks or is weak in)
- talkingPoints: array of strings (suggested points Rodney should emphasize in an interview)

Job description:
---
{jobDescription.Trim()}
---

Resume context:
---
{resumeContext}
---

Return ONLY valid JSON. No markdown fences, no code blocks, no explanation. Just the JSON object.";

        var requestBody = new
        {
            model = _config["Anthropic:Model"] ?? "claude-sonnet-4-6",
            max_tokens = 1024,
            messages = new[] { new { role = "user", content = prompt } }
        };

        try
        {
            var response = await _anthropicClient.PostMessagesAsync(requestBody, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Anthropic API failed for job match. StatusCode: {StatusCode}", (int)response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadFromJsonAsync<AnthropicResponse>(cancellationToken);
            var content = json?.Content?.FirstOrDefault(b => b.Type == "text")?.Text?.Trim();
            if (string.IsNullOrEmpty(content))
                return null;

            return ParseJobMatchResponse(content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Anthropic job match analysis");
            return null;
        }
    }

    private static JobMatchResponse? ParseJobMatchResponse(string jsonContent)
    {
        try
        {
            // Strip markdown fences if Claude included them despite instructions
            var trimmed = jsonContent.Trim();
            if (trimmed.StartsWith("```"))
            {
                var start = trimmed.IndexOf('\n') + 1;
                var end = trimmed.LastIndexOf("```");
                if (end > start)
                    trimmed = trimmed[start..end].Trim();
            }

            using var doc = JsonDocument.Parse(trimmed);
            var root = doc.RootElement;

            var score = 0;
            if (root.TryGetProperty("matchScore", out var scoreEl))
                score = scoreEl.TryGetInt32(out var s) ? s : 0;

            return new JobMatchResponse
            {
                MatchScore = Math.Clamp(score, 0, 100),
                SkillsAligned = GetStringArray(root, "skillsAligned"),
                Gaps = GetStringArray(root, "gaps"),
                TalkingPoints = GetStringArray(root, "talkingPoints")
            };
        }
        catch
        {
            return null;
        }
    }

    private static List<string> GetStringArray(JsonElement root, string propertyName)
    {
        var list = new List<string>();
        if (!root.TryGetProperty(propertyName, out var arr) || arr.ValueKind != JsonValueKind.Array)
            return list;
        foreach (var item in arr.EnumerateArray())
        {
            var s = item.GetString();
            if (!string.IsNullOrWhiteSpace(s))
                list.Add(s.Trim());
        }
        return list;
    }

    private static JobMatchResponse FallbackResponse() => new()
    {
        MatchScore = 0,
        SkillsAligned = new List<string>(),
        Gaps = new List<string> { "Could not parse analysis. Please try again." },
        TalkingPoints = new List<string>()
    };

    // ReSharper disable once ClassNeverInstantiated.Local
    private sealed class AnthropicResponse
    {
        public List<ContentBlock>? Content { get; set; }
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private sealed class ContentBlock
    {
        public string? Type { get; set; }
        public string? Text { get; set; }
    }
}
