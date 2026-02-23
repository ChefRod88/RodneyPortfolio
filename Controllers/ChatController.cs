using Microsoft.AspNetCore.Mvc;
using RodneyPortfolio.Models;
using RodneyPortfolio.Services;

namespace RodneyPortfolio.Controllers;

/// <summary>
/// REST API controller for the "Ask Rodney" chatbot.
/// POST /api/chat - Sends a message and receives an AI-generated reply about Rodney.
/// POST /api/chat/job-match - Analyzes job description compatibility with Rodney's resume.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private const int JobDescriptionMaxLength = 4000;

    private readonly IAIChatService _aiService;
    private readonly IJobMatchService _jobMatchService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IAIChatService aiService, IJobMatchService jobMatchService, ILogger<ChatController> logger)
    {
        _aiService = aiService;
        _jobMatchService = jobMatchService;
        _logger = logger;
    }

    /// <summary>
    /// Processes a chat message and returns an AI-generated reply about Rodney.
    /// Validates input and applies content filtering before calling the AI service.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] ChatRequest request, CancellationToken cancellationToken)
    {
        if (request?.Message == null)
        {
            return BadRequest(new { error = "Message is required." });
        }

        // Input validation - AI safety guardrail
        var validationError = InputValidator.GetValidationError(request.Message);
        if (validationError != null)
        {
            _logger.LogDebug("Chat input validation failed: {Error}", validationError);
            return BadRequest(new { error = validationError });
        }

        // Content filter - block inappropriate content
        if (ContentFilter.IsBlocked(request.Message))
        {
            _logger.LogDebug("Chat message blocked by content filter");
            return BadRequest(new { error = "Your message could not be processed." });
        }

        try
        {
            var reply = await _aiService.GetReplyAsync(request.Message.Trim(), request.Mode, cancellationToken);
            return Ok(new ChatResponse { Reply = reply });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat message");
            return StatusCode(500, new { error = "Something went wrong. Please try again." });
        }
    }

    /// <summary>
    /// Analyzes a job description against Rodney's resume. Returns match score, skills alignment, gaps, and talking points.
    /// </summary>
    [HttpPost("job-match")]
    public async Task<IActionResult> JobMatch([FromBody] JobMatchRequest request, CancellationToken cancellationToken)
    {
        if (request?.JobDescription == null)
        {
            return BadRequest(new { error = "Job description is required." });
        }

        var trimmed = request.JobDescription.Trim();
        if (trimmed.Length == 0)
        {
            return BadRequest(new { error = "Job description cannot be empty." });
        }

        if (trimmed.Length > JobDescriptionMaxLength)
        {
            return BadRequest(new { error = $"Job description must be {JobDescriptionMaxLength} characters or less." });
        }

        try
        {
            var result = await _jobMatchService.AnalyzeAsync(trimmed, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing job match");
            return StatusCode(500, new { error = "Something went wrong. Please try again." });
        }
    }
}
