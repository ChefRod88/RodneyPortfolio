using System.ComponentModel.DataAnnotations;

namespace RodneyPortfolio.Models;

/// <summary>
/// Request body for POST /api/chat.
/// </summary>
public class ChatRequest
{
    [Required]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Chat mode: "recruiter" (default), "engineer", or "interview".
    /// </summary>
    public string? Mode { get; set; }
}
