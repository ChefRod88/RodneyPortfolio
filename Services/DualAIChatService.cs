namespace RodneyPortfolio.Services;

/// <summary>
/// Orchestrates parallel calls to OpenAI and Anthropic, merging replies into one richer response.
/// Registered as IAIChatService so ChatController is unaware of the duality.
/// </summary>
public class DualAIChatService : IAIChatService
{
    private readonly OpenAIChatService _openAi;
    private readonly AnthropicChatService _anthropic;
    private readonly IResumeContextLoader _resumeLoader;
    private readonly ILogger<DualAIChatService> _logger;

    public DualAIChatService(
        OpenAIChatService openAi,
        AnthropicChatService anthropic,
        IResumeContextLoader resumeLoader,
        ILogger<DualAIChatService> logger)
    {
        _openAi = openAi;
        _anthropic = anthropic;
        _resumeLoader = resumeLoader;
        _logger = logger;
    }

    public async Task<string> GetReplyAsync(string userMessage, string? mode = null, CancellationToken cancellationToken = default)
    {
        var resumeContext = await _resumeLoader.LoadAsync(cancellationToken);

        var openAiTask  = _openAi.TryGetReplyAsync(userMessage, mode, resumeContext, cancellationToken);
        var claudeTask  = _anthropic.TryGetReplyAsync(userMessage, mode, resumeContext, cancellationToken);

        await Task.WhenAll(openAiTask, claudeTask);

        var openAiReply = openAiTask.Result;
        var claudeReply = claudeTask.Result;

        if (!string.IsNullOrEmpty(openAiReply) && !string.IsNullOrEmpty(claudeReply))
            return $"{openAiReply}\n\n---\n\n*Additional perspective:*\n\n{claudeReply}";

        if (!string.IsNullOrEmpty(openAiReply))
            return openAiReply;

        if (!string.IsNullOrEmpty(claudeReply))
            return claudeReply;

        _logger.LogWarning("Both OpenAI and Anthropic failed. Returning demo fallback.");
        return GetDemoFallback(userMessage);
    }

    private static string GetDemoFallback(string userMessage)
    {
        var q = userMessage.Trim().ToLowerInvariant();

        if (q.Contains("work") || q.Contains("employer") || q.Contains("company") || q.Contains("job"))
            return "Rodney works in three concurrent roles: Technical Support Specialist at Canon IT Services (March 2025–Present), Freelance Enterprise Web Developer (Feb 2023–Present), and Minister of Technology at New Bethel Missionary Baptist Church (October 2025–Present).";

        if (q.Contains("skill") || q.Contains("tool") || q.Contains("technolog") || q.Contains("stack"))
            return "Rodney's technical skills include C#, ASP.NET Core, .NET MAUI, Entity Framework Core, SQL Server, Git/GitHub, HTML5, CSS3, JavaScript, Azure, AWS, IIS, Docker, xUnit, Moq, Vitest, Salesforce, Oracle PeopleSoft, PowerShell, Active Directory, and OpenAI/Anthropic API integration.";

        if (q.Contains("education") || q.Contains("degree") || q.Contains("school"))
            return "Rodney is pursuing a B.S. in Software Engineering at Western Governors University, expected December 2026. He holds AWS CCP, ITIL® 4 Foundation, CompTIA Project+, and Google IT Support Professional certifications.";

        if (q.Contains("contact") || q.Contains("email") || q.Contains("linkedin"))
            return "You can reach Rodney at chefrodneyachery@gmail.com or on LinkedIn at linkedin.com/in/rodneyachery.";

        if (q.Contains("project") || q.Contains("portfolio") || q.Contains("built"))
            return "Rodney's projects include his portfolio website (ASP.NET Core, Razor Pages), the Mini-D365-CRM (ASP.NET Core 10, OData v4, Azure), the Student Progress Tracker (.NET MAUI, WGU capstone), and the Ask Rodney AI Chatbot with dual-AI orchestration.";

        return "Rodney Chery is an Enterprise Application Developer with hands-on experience across the full SDLC. He builds with ASP.NET Core, C#, SQL Server, and Azure. For more details, check his portfolio at rodneyachery.com.";
    }
}
