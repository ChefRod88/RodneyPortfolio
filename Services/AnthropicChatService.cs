using System.Net.Http.Json;

namespace RodneyPortfolio.Services;

/// <summary>
/// AI chat service backed by Anthropic's Claude API.
/// Mirrors OpenAIChatService behavior; used standalone or via DualAIChatService.
/// </summary>
public class AnthropicChatService : IAIChatService
{
    private readonly IResumeContextLoader _resumeLoader;
    private readonly IConfiguration _config;
    private readonly IAnthropicClient _anthropicClient;
    private readonly ILogger<AnthropicChatService> _logger;

    private const int MaxResponseTokens = 512;

    public AnthropicChatService(
        IResumeContextLoader resumeLoader,
        IConfiguration config,
        IAnthropicClient anthropicClient,
        ILogger<AnthropicChatService> logger)
    {
        _resumeLoader = resumeLoader;
        _config = config;
        _anthropicClient = anthropicClient;
        _logger = logger;
    }

    public async Task<string> GetReplyAsync(string userMessage, string? mode = null, CancellationToken cancellationToken = default)
    {
        var apiKey = _config["Anthropic:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("Anthropic API key not configured");
            return "The chatbot is not configured. Please contact the site owner.";
        }

        var resumeContext = await _resumeLoader.LoadAsync(cancellationToken);
        var result = await TryGetReplyAsync(userMessage, mode, resumeContext, cancellationToken);
        return result ?? await GetDemoResponseAsync(userMessage, resumeContext, cancellationToken);
    }

    /// <summary>
    /// Calls Anthropic and returns the reply, or null on any API failure.
    /// Used by DualAIChatService to run both providers in parallel.
    /// </summary>
    public async Task<string?> TryGetReplyAsync(string userMessage, string? mode, string resumeContext, CancellationToken cancellationToken)
    {
        var systemPrompt = BuildSystemPrompt(resumeContext, mode);

        var requestBody = new
        {
            model = _config["Anthropic:Model"] ?? "claude-sonnet-4-6",
            max_tokens = MaxResponseTokens,
            system = systemPrompt,
            messages = new[] { new { role = "user", content = userMessage } }
        };

        try
        {
            var response = await _anthropicClient.PostMessagesAsync(requestBody, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning(
                    "Anthropic API failed. StatusCode: {StatusCode}, Response: {Response}.",
                    (int)response.StatusCode,
                    errorBody);
                return null;
            }

            var json = await response.Content.ReadFromJsonAsync<AnthropicResponse>(cancellationToken);
            var reply = json?.Content?.FirstOrDefault(b => b.Type == "text")?.Text?.Trim();
            return string.IsNullOrEmpty(reply) ? null : reply;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Anthropic API.");
            return null;
        }
    }

    private static string GetModeInstructions(string? mode)
    {
        var m = (mode ?? "recruiter").Trim().ToLowerInvariant();
        return m switch
        {
            "engineer" => @"MODE: Engineer. Go deeper technically. Mention specific tools, patterns, and tradeoffs. Include code snippets when relevant (e.g., C#, ASP.NET, OpenAI integration). Emphasize architecture, debugging approach, and technical decisions. Avoid oversimplifying.",
            "interview" => @"MODE: Interview. You are an interviewer. Ask behavioral, system design, or debugging questions. After the user answers, give brief constructive feedback. Stay in character. Use Rodney's background for relevant follow-ups (e.g., Canon, kitchens-to-tech). If the user asks about Rodney, answer as usual; if they say they're ready for an interview, start asking questions.",
            _ => @"MODE: Recruiter. Be concise. Emphasize impact, teamwork, and soft skills. Avoid jargon. Focus on outcomes and metrics recruiters care about."
        };
    }

    private static string BuildSystemPrompt(string resumeContext, string? mode)
    {
        var today = DateTime.UtcNow.ToString("MMMM yyyy");
        var modeInstructions = GetModeInstructions(mode);

        return $@"You are a friendly assistant representing Rodney Chery. You have access to the following document about him. Answer questions as if you're a knowledgeable colleague who has read his resume and can discuss him naturally—like ChatGPT when someone uploads a document and asks varied questions.

{modeInstructions}

Current date for calculations: {today}

--- RESUME & ABOUT CONTENT ---
{resumeContext}
--- END ---

CRITICAL: Answer the SPECIFIC question asked. Different questions deserve different answers.
- ""Where does he work?"" → Short, direct: ""Canon Information Technology Services"" or ""He works at Canon."" Don't repeat his full experience paragraph.
- ""What's his experience?"" → Fuller narrative about his role and background.
- ""What tools does he use?"" → List the tools/technologies from the context.
- ""How long has he been at Canon?"" → Calculate from the dates (started March 2025; use the current date provided above).
- ""What are his strengths?"" → Extract and summarize strengths—don't paste the experience block.
- ""What are his weaknesses?"" → Infer reasonably from the context if not stated; it's okay to say what might be areas of growth.
- For project questions (chatbot, portfolio, architecture, tech stack): Provide architecture overview, tech stack, key decisions, tradeoffs, and deployment strategy from the PROJECT DEEP-DIVES section. Code snippets when relevant.

Behave like generative AI with a loaded document: infer, calculate, extract, and tailor each response to the exact question. Vary your response style—short for simple questions, more depth when asked. Sound human and conversational, not robotic. Never give the same generic block of text for different questions. Speak in third person about Rodney. Stay grounded in the context; don't invent employers, dates, or credentials not mentioned.";
    }

    private static Task<string> GetDemoResponseAsync(string userMessage, string resumeContext, CancellationToken cancellationToken)
    {
        var q = userMessage.Trim().ToLowerInvariant();
        string reply;

        if (q.Contains("work") || q.Contains("employer") || q.Contains("where does") || q.Contains("company") || q.Contains("job"))
            reply = "Rodney works at Canon Information Technology Services as a Technical Support Inkjet Tier 1. He started in March 2025 and troubleshoots proprietary software, hardware environments, and enterprise applications.";
        else if (q.Contains("experience") || q.Contains("background") || q.Contains("career"))
            reply = "Rodney is a Technical Support Analyst at Canon IT Services with a background in professional kitchens. He brings composure under pressure, disciplined troubleshooting, and strong service mindset to enterprise support. His journey from kitchens to tech reflects adaptability, resilience, and growth.";
        else if (q.Contains("skill") || q.Contains("tool") || q.Contains("technolog") || q.Contains("stack"))
            reply = "Rodney's technical skills include C#, ASP.NET Core, Entity Framework Core, SQL Server, Git/GitHub, HTML, CSS, JavaScript, TypeScript, Angular, Razor Pages, Azure, Docker, and OpenAI API integration. He also has strong soft skills in technical communication, problem-solving under pressure, and customer empathy.";
        else if (q.Contains("education") || q.Contains("degree") || q.Contains("school") || q.Contains("graduate"))
            reply = "Rodney is pursuing a B.S. in Software Engineering at Western Governors University, expected December 2026. The program is self-paced and competency-based, with focus on full-stack development, databases, and software architecture.";
        else if (q.Contains("contact") || q.Contains("email") || q.Contains("linkedin") || q.Contains("reach"))
            reply = "You can reach Rodney at rodney@globalrcdev.com, on LinkedIn at linkedin.com/in/rodneyachery, or via his portfolio at rodneyachery.com.";
        else if (q.Contains("strength") || q.Contains("differentiate") || q.Contains("what makes"))
            reply = "Rodney's strengths include composure under pressure, disciplined troubleshooting, strong service mindset, meticulous documentation, and clear communication. His intersection of soft skills and technical execution sets him apart.";
        else if (q.Contains("how long") || q.Contains("tenure") || q.Contains("years at"))
            reply = "Rodney started at Canon Information Technology Services in March 2025.";
        else if (q.Contains("project") || q.Contains("portfolio") || q.Contains("built"))
            reply = "Rodney's projects include his portfolio website (rodneyachery.com) built with ASP.NET Core and Razor Pages, and the Ask Rodney AI Chatbot—a C# backend with OpenAI API integration, prompt engineering, and AI safety practices.";
        else
            reply = "Rodney Chery is a Technical Support Analyst at Canon IT Services with a background in professional kitchens. He brings composure under pressure, disciplined troubleshooting, and strong service mindset to enterprise support. For more details, check his resume or portfolio at rodneyachery.com.";

        return Task.FromResult(reply);
    }

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
