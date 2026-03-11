using System.Text.RegularExpressions;

namespace RodneyPortfolio.Services;

/// <summary>
/// Content filter for AI safety. Blocks inappropriate content before it reaches the AI.
/// Configurable block list for profanity and other unwanted content.
/// </summary>
public class ContentFilter : IContentFilter
{
    /// <summary>
    /// Words and phrases that should block the message. Expand as needed for your use case.
    /// </summary>
    private static readonly string[] BlockedTerms =
    {
        // Add terms as needed - expand as needed for your use case
        "fuck", "shit", "asshole", "bitch", "damn" // example profanity
    };

    private static readonly Regex[] _blockPatterns = BlockedTerms
        .Select(t => new Regex(
            $@"\b{Regex.Escape(t)}\b",
            RegexOptions.Compiled,
            TimeSpan.FromMilliseconds(100)))
        .ToArray();

    /// <summary>
    /// Returns true if the message should be blocked (contains inappropriate content).
    /// </summary>
    public bool IsBlocked(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return false;

        var lower = message.Trim().ToLowerInvariant();

        foreach (var regex in _blockPatterns)
        {
            try { if (regex.IsMatch(lower)) return true; }
            catch (RegexMatchTimeoutException) { return true; } // treat timeout as blocked
        }

        return false;
    }
}
