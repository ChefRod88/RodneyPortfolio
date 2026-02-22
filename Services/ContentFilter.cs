using System.Text.RegularExpressions;

namespace RodneyPortfolio.Services;

/// <summary>
/// Content filter for AI safety. Blocks inappropriate content before it reaches the AI.
/// Configurable block list for profanity and other unwanted content.
/// </summary>
public static class ContentFilter
{
    /// <summary>
    /// Words and phrases that should block the message. Expand as needed for your use case.
    /// </summary>
    private static readonly string[] BlockedTerms =
    {
        // Add terms as needed - keeping minimal for portfolio demo
        "fuck", "shit", "asshole", "bitch", "damn" // example profanity
    };

    /// <summary>
    /// Returns true if the message should be blocked (contains inappropriate content).
    /// </summary>
    public static bool IsBlocked(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return false;

        var lower = message.Trim().ToLowerInvariant();

        foreach (var term in BlockedTerms)
        {
            // Word boundary check - avoid false positives (e.g., "class" containing "ass")
            var pattern = $@"\b{Regex.Escape(term)}\b";
            if (Regex.IsMatch(lower, pattern))
                return true;
        }

        return false;
    }
}
