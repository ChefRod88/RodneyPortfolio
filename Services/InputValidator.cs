using System.Text.RegularExpressions;

namespace RodneyPortfolio.Services;

/// <summary>
/// Validates user input before sending to the AI. Implements AI safety guardrails:
/// max length, prompt injection pattern blocking, and basic sanitization.
/// </summary>
public static class InputValidator
{
    private const int MaxLength = 500;

    /// <summary>
    /// Patterns that may indicate prompt injection attempts. Block these to protect the system prompt.
    /// </summary>
    private static readonly string[] BlockedPatterns =
    {
        "ignore previous",
        "ignore all previous",
        "disregard",
        "forget everything",
        "system:",
        "system prompt",
        "you are now",
        "act as",
        "pretend you are",
        "new instructions",
        "override",
        "bypass",
        "jailbreak",
        "###"
    };

    /// <summary>
    /// Validates the user message. Returns true if the input is safe to send to the AI.
    /// </summary>
    public static bool IsValid(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return false;

        var trimmed = message.Trim();

        if (trimmed.Length > MaxLength)
            return false;

        var lower = trimmed.ToLowerInvariant();
        foreach (var pattern in BlockedPatterns)
        {
            if (lower.Contains(pattern))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Returns a user-friendly error message when validation fails, or null if valid.
    /// </summary>
    public static string? GetValidationError(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return "Please enter a question.";

        var trimmed = message.Trim();
        if (trimmed.Length > MaxLength)
            return $"Message must be {MaxLength} characters or less.";

        var lower = trimmed.ToLowerInvariant();
        foreach (var pattern in BlockedPatterns)
        {
            if (lower.Contains(pattern))
                return "Your message contains content that cannot be processed.";
        }

        return null;
    }
}
