using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using RodneyPortfolio.Models;

namespace RodneyPortfolio.Services;

/// <summary>
/// Analyzes job descriptions against Rodney's resume using OpenAI to produce match score, skills alignment, gaps, and talking points.
/// </summary>
public class JobMatchService : IJobMatchService
{
    private readonly IResumeContextLoader _resumeLoader;
    private readonly IConfiguration _config;
    private readonly IOpenAIClient _openAiClient;
    private readonly ILogger<JobMatchService> _logger;

    public JobMatchService(
        IResumeContextLoader resumeLoader,
        IConfiguration config,
        IOpenAIClient openAiClient,
        ILogger<JobMatchService> logger)
    {
        _resumeLoader = resumeLoader;
        _config = config;
        _openAiClient = openAiClient;
        _logger = logger;
    }

    public async Task<JobMatchResponse> AnalyzeAsync(string jobDescription, CancellationToken cancellationToken = default)
    {
        try
        {
            var apiKey = _config["OpenAI:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogWarning("OpenAI API key not configured");
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in JobMatchService.AnalyzeAsync");
            return FallbackResponse();
        }
    }

    /// <summary>
    /// Calls OpenAI for job match analysis and returns the result, or null on any API failure.
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

Return ONLY valid JSON, no other text.";

        var requestBody = new
        {
            model = _config["OpenAI:Model"] ?? "gpt-4o-mini",
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            max_tokens = 1024,
            response_format = new { type = "json_object" }
        };

        try
        {
            var response = await _openAiClient.PostChatCompletionsAsync(requestBody, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("OpenAI API failed for job match. StatusCode: {StatusCode}", (int)response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadFromJsonAsync<OpenAIJobMatchResponse>(cancellationToken);
            var content = json?.Choices?.FirstOrDefault()?.Message?.Content?.Trim();
            if (string.IsNullOrEmpty(content))
                return null;

            return ParseJobMatchResponse(content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in job match analysis");
            return null;
        }
    }

    private static JobMatchResponse ParseJobMatchResponse(string jsonContent)
    {
        try
        {
            using var doc = JsonDocument.Parse(jsonContent);
            var root = doc.RootElement;

            var score = 0;
            if (root.TryGetProperty("matchScore", out var scoreEl))
                score = scoreEl.TryGetInt32(out var s) ? s : 0;

            var skills = GetStringArray(root, "skillsAligned");
            var gaps = GetStringArray(root, "gaps");
            var talkingPoints = GetStringArray(root, "talkingPoints");

            return new JobMatchResponse
            {
                MatchScore = Math.Clamp(score, 0, 100),
                SkillsAligned = skills,
                Gaps = gaps,
                TalkingPoints = talkingPoints
            };
        }
        catch
        {
            return FallbackResponse();
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

    private sealed class OpenAIJobMatchResponse
    {
        public List<Choice>? Choices { get; set; }
    }

    private sealed class Choice
    {
        public Message? Message { get; set; }
    }

    private sealed class Message
    {
        public string? Content { get; set; }
    }
}
