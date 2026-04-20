
namespace ChurchWebsite.Models;

/// <summary>Pexels API configuration. Set ApiKey via user secrets or environment variable Pexels__ApiKey.</summary>
public class PexelsOptions
{
    public const string SectionName = "Pexels";

    /// <summary>API key from https://www.pexels.com/api/ — never commit real keys.</summary>
    public string ApiKey { get; set; } = string.Empty;
}
