using System.Net.Http.Json;

namespace RodneyPortfolio.Services;

/// <summary>
/// AI chat service that uses OpenAI's Chat Completions API to answer questions about Rodney.
/// Supports demo mode when no API key is configured.
/// </summary>
public class OpenAIChatService : IAIChatService
{
    private readonly IResumeContextLoader _resumeLoader;
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<OpenAIChatService> _logger;

    private const int MaxResponseTokens = 512;

    public OpenAIChatService(
        IResumeContextLoader resumeLoader,
        IConfiguration config,
        IHttpClientFactory httpClientFactory,
        ILogger<OpenAIChatService> logger)
    {
        _resumeLoader = resumeLoader;
        _config = config;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<(string Reply, string Source)> GetReplyAsync(string userMessage, CancellationToken cancellationToken = default)
    {
        var apiKey = _config["OpenAI:ApiKey"];
        var useDemoMode = _config.GetValue<bool>("OpenAI:UseDemoMode");

        // Demo mode: no API key or explicitly enabled - return canned responses
        if (string.IsNullOrWhiteSpace(apiKey) || useDemoMode)
        {
            var demoReply = await GetDemoResponseAsync(userMessage, cancellationToken);
            return (demoReply, "demo");
        }

        // Call OpenAI API
        var resumeContext = await _resumeLoader.LoadAsync(cancellationToken);
        var systemPrompt = BuildSystemPrompt(resumeContext);

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
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var response = await client.PostAsJsonAsync(
                "https://api.openai.com/v1/chat/completions",
                requestBody,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("OpenAI API returned {StatusCode}", response.StatusCode);
                return ("I'm having trouble connecting right now. Please try again later or check Rodney's resume directly.", "demo");
            }

            var json = await response.Content.ReadFromJsonAsync<OpenAIResponse>(cancellationToken);
            var reply = json?.Choices?.FirstOrDefault()?.Message?.Content?.Trim();

            if (string.IsNullOrEmpty(reply))
                return ("I couldn't generate a response. Please try rephrasing your question.", "demo");

            return (reply, "api");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling OpenAI API");
            return ("Something went wrong. Please try again or reach out to Rodney directly.", "demo");
        }
    }

    /// <summary>
    /// Builds the system prompt with resume context. Prompt engineering: grounds the model
    /// in the provided content while allowing inference, expansion, and natural conversation.
    /// </summary>
    private static string BuildSystemPrompt(string resumeContext)
    {
        var today = DateTime.UtcNow.ToString("MMMM yyyy");
        return $@"You are a friendly assistant representing Rodney Chery. You have access to the following document about him. Answer questions as if you're a knowledgeable colleague who has read his resume and can discuss him naturally—like ChatGPT when someone uploads a document and asks varied questions.

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

Behave like generative AI with a loaded document: infer, calculate, extract, and tailor each response to the exact question. Vary your response style—short for simple questions, more depth when asked. Sound human and conversational, not robotic. Never give the same generic block of text for different questions. Speak in third person about Rodney. Stay grounded in the context; don't invent employers, dates, or credentials not mentioned.";

    }

    /// <summary>
    /// Returns predefined responses when no API key is configured. Uses keyword matching
    /// to provide relevant answers from the resume context.
    /// </summary>
    private async Task<string> GetDemoResponseAsync(string userMessage, CancellationToken cancellationToken = default)
    {
        var msg = userMessage.Trim().ToLowerInvariant();

        if (msg.Contains("experience") || msg.Contains("work") || msg.Contains("job") || msg.Contains("canon"))
        {
            return "Rodney is a Technical Support Inkjet Tier 1 at Canon Information Technology Services. He troubleshoots proprietary software, hardware environments, and enterprise applications by systematically isolating variables and guiding issues through structured resolution. He brings a background from professional kitchens where he learned discipline and composure under pressure.";
        }

        if (msg.Contains("background") || msg.Contains("story") || msg.Contains("kitchen") || msg.Contains("chef"))
        {
            return "Rodney transitioned from professional kitchens to technology. In kitchens, he learned discipline, communication, and how to remain steady under pressure. Those lessons define his professional identity and inform his approach to enterprise technical support.";
        }

        if (msg.Contains("skill") || msg.Contains("technologies") || msg.Contains("tech"))
        {
            return "Rodney has a strong technical foundation: C#, ASP.NET Core, SQL Server, Entity Framework Core, and Razor Pages. He's also skilled in HTML, CSS, JavaScript, and TypeScript. His soft skills include technical communication, problem-solving under pressure, and customer empathy.";
        }

        if (msg.Contains("education") || msg.Contains("degree") || msg.Contains("school") || msg.Contains("wgu"))
        {
            return "Rodney is pursuing a B.S. in Software Engineering at Western Governors University (WGU), with expected graduation in December 2026.";
        }

        if (msg.Contains("approach") || msg.Contains("troubleshoot") || msg.Contains("method"))
        {
            return "Rodney's approach to troubleshooting is intentional and analytical. He assesses severity, understands user impact, gathers evidence, and determines whether issues are environmental, configuration-based, user-driven, or systemic. He maintains meticulous documentation and values transparency, especially during escalations.";
        }

        if (msg.Contains("contact") || msg.Contains("email") || msg.Contains("reach") || msg.Contains("hire"))
        {
            return "You can reach Rodney at chefrodneyachery@gmail.com or connect with him on LinkedIn at linkedin.com/in/rodneyachery. His portfolio and GitHub are also available on this site.";
        }

        // Default: suggest they ask something more specific
        return "I can answer questions about Rodney's background, experience, skills, education, and approach to work. Try asking something like 'What's his experience?' or 'Tell me about his skills.' For his full resume, use the Download CV button above.";
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
