using Microsoft.AspNetCore.Mvc;
using RodneyPortfolio.Models;
using RodneyPortfolio.Services;

namespace RodneyPortfolio.Controllers;

/// <summary>
/// REST API controller for the "Ask Rodney" chatbot.
/// POST /api/chat - Sends a message and receives an AI-generated reply about Rodney.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IAIChatService _aiService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IAIChatService aiService, ILogger<ChatController> logger)
    {
        _aiService = aiService;
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
            var (reply, source) = await _aiService.GetReplyAsync(request.Message.Trim(), cancellationToken);
            return Ok(new ChatResponse { Reply = reply, Source = source });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat message");
            return StatusCode(500, new { error = "Something went wrong. Please try again." });
        }
    }
}
