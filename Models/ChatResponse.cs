namespace RodneyPortfolio.Models;

/// <summary>
/// Response body for POST /api/chat.
/// </summary>
public class ChatResponse
{
    public string Reply { get; set; } = string.Empty;
    /// <summary>Indicates whether the reply came from the OpenAI API ("api") or demo mode ("demo").</summary>
    public string Source { get; set; } = "demo";
}
