using System.Net.Http.Json;

namespace RodneyPortfolio.Services;

/// <summary>
/// AI chat service that uses OpenAI's Chat Completions API to answer questions about Rodney.
/// Requires an API key configured via User Secrets (local) or GitHub Secrets (production).
/// </summary>
public class OpenAIChatService : IAIChatService
{
    private readonly IResumeContextLoader _resumeLoader;
    private readonly IConfiguration _config;
    private readonly IOpenAIClient _openAiClient;
    private readonly ILogger<OpenAIChatService> _logger;

    private const int MaxResponseTokens = 512;

    public OpenAIChatService(
        IResumeContextLoader resumeLoader,
        IConfiguration config,
        IOpenAIClient openAiClient,
        ILogger<OpenAIChatService> logger)
    {
        _resumeLoader = resumeLoader;
        _config = config;
        _openAiClient = openAiClient;
        _logger = logger;
    }

    public async Task<string> GetReplyAsync(string userMessage, string? mode = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var apiKey = _config["OpenAI:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogWarning("OpenAI API key not configured");
                return "The chatbot is not configured. Please contact the site owner.";
            }

            var resumeContext = await _resumeLoader.LoadAsync(cancellationToken);
            var result = await TryGetReplyAsync(userMessage, mode, resumeContext, cancellationToken);
            return result ?? await GetDemoResponseAsync(userMessage, resumeContext, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in OpenAIChatService.GetReplyAsync");
            return await GetDemoResponseAsync(userMessage, string.Empty, cancellationToken);
        }
    }

    /// <summary>
    /// Calls OpenAI and returns the reply, or null on any API failure.
    /// Used by DualAIChatService to run both providers in parallel.
    /// </summary>
    public async Task<string?> TryGetReplyAsync(string userMessage, string? mode, string resumeContext, CancellationToken cancellationToken)
    {
        var systemPrompt = BuildSystemPrompt(resumeContext, mode);

        var requestBody = new
        {
            model = _config["OpenAI:Model"] ?? "gpt-4o-mini",
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userMessage }
            },
            max_tokens = MaxResponseTokens
        };

        try
        {
            var response = await _openAiClient.PostChatCompletionsAsync(requestBody, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning(
                    "OpenAI API failed. StatusCode: {StatusCode}, Response: {Response}. Falling back to demo response.",
                    (int)response.StatusCode,
                    errorBody);
                return null;
            }

            var json = await response.Content.ReadFromJsonAsync<OpenAIResponse>(cancellationToken);
            var reply = json?.Choices?.FirstOrDefault()?.Message?.Content?.Trim();
            return string.IsNullOrEmpty(reply) ? null : reply;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling OpenAI API.");
            return null;
        }
    }

    /// <summary>
    /// Returns mode-specific prompt instructions for tone, depth, and emphasis.
    /// </summary>
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

    /// <summary>
    /// Builds the system prompt with resume context. Prompt engineering: grounds the model
    /// in the provided content while allowing inference, expansion, and natural conversation.
    /// Mode-specific instructions adjust tone, depth, and emphasis.
    /// </summary>
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

    /// <summary>
    /// Returns a demo response based on resume context when the OpenAI API fails (e.g., 429 quota, 401, network error).
    /// Uses keyword matching to provide relevant answers from the context. API errors are logged but not shown to the user.
    /// </summary>
    private static Task<string> GetDemoResponseAsync(string userMessage, string resumeContext, CancellationToken cancellationToken)
    {
        var q = userMessage.Trim().ToLowerInvariant();
        string reply;

        if (q.Contains("work") || q.Contains("employer") || q.Contains("where does") || q.Contains("company") || q.Contains("job"))
            reply = "Rodney works in three concurrent roles: Technical Support Specialist at Canon Information Technology Services (March 2025–Present), Freelance Enterprise Web Developer (Feb 2023–Present), and Minister of Technology at New Bethel Missionary Baptist Church (October 2025–Present).";
        else if (q.Contains("experience") || q.Contains("background") || q.Contains("career"))
            reply = "Rodney is an Enterprise Application Developer with hands-on experience across the full SDLC. He builds production-style web applications with ASP.NET Core, C#, SQL Server, and Azure while working concurrently as a Technical Support Specialist at Canon IT Services and as a freelance developer for non-profits.";
        else if (q.Contains("skill") || q.Contains("tool") || q.Contains("technolog") || q.Contains("stack"))
            reply = "Rodney's technical skills include C#, ASP.NET Core, .NET MAUI, Entity Framework Core, SQL Server, Git/GitHub, HTML5, CSS3, JavaScript, Razor Pages, Azure, AWS, IIS, Docker, xUnit, Moq, Vitest, Salesforce, Oracle PeopleSoft, PowerShell, Active Directory, and OpenAI/Anthropic API integration.";
        else if (q.Contains("education") || q.Contains("degree") || q.Contains("school") || q.Contains("graduate"))
            reply = "Rodney is pursuing a B.S. in Software Engineering at Western Governors University, expected December 2026. He holds AWS Certified Cloud Practitioner, ITIL® 4 Foundation, CompTIA Project+, Google IT Support Professional, and Google Cloud: Introduction to Generative AI certifications.";
        else if (q.Contains("contact") || q.Contains("email") || q.Contains("linkedin") || q.Contains("reach"))
            reply = "You can reach Rodney at chefrodneyachery@gmail.com, on LinkedIn at linkedin.com/in/rodneyachery, or via his portfolio at rodneyachery.com.";
        else if (q.Contains("strength") || q.Contains("differentiate") || q.Contains("what makes"))
            reply = "Rodney's strengths include full SDLC ownership, interface-driven architecture, dependency injection, AI-augmented development workflows, root-cause analysis, and clear stakeholder communication. He combines builder mindset with production-minded support discipline.";
        else if (q.Contains("how long") || q.Contains("tenure") || q.Contains("years at"))
            reply = "Rodney started at Canon Information Technology Services in March 2025. He has been freelancing as a web developer since February 2023 and serving as Minister of Technology at New Bethel since October 2025.";
        else if (q.Contains("project") || q.Contains("portfolio") || q.Contains("built"))
            reply = "Rodney's projects include his portfolio (rodneyachery.com) with AI chatbot and job match features, production sites such as New Bethel Church and Prime Medical Group, the Mini-D365-CRM (ASP.NET Core 10, OData v4, Azure), and ongoing refactors for Drumgo Publishing and Florida Theological Seminary.";
        else
            reply = "Rodney Chery is an Enterprise Application Developer with experience across the full SDLC. He builds with ASP.NET Core, C#, SQL Server, and Azure, and works concurrently as a Technical Support Specialist at Canon IT Services and as a Freelance Web Developer. For more details, check his portfolio at rodneyachery.com.";

        return Task.FromResult(reply);
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private sealed class OpenAIResponse
    {
        public List<Choice>? Choices { get; set; }
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private sealed class Choice
    {
        public Message? Message { get; set; }
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private sealed class Message
    {
        public string? Content { get; set; }
    }
}
