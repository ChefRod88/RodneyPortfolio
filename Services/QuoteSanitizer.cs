using System.Text.RegularExpressions;

namespace RodneyPortfolio.Services;

/// <summary>
/// Strips characters that enable email header injection or log injection.
/// Single authoritative implementation used by both QuoteLogService and QuoteEmailService.
/// </summary>
internal static class QuoteSanitizer
{
    internal static string Sanitize(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        // Remove CR, LF, null bytes, and other ASCII control characters
        var cleaned = Regex.Replace(value, @"[\r\n\0\x01-\x08\x0b\x0c\x0e-\x1f\x7f]", " ");
        // Collapse runs of whitespace to a single space
        cleaned = Regex.Replace(cleaned, @"[ \t]{2,}", " ");
        return cleaned.Trim();
    }
}
