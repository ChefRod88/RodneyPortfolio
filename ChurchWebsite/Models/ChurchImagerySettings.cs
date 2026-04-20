namespace ChurchWebsite.Models;

/// <summary>Maps imagery slots to Pexels search queries (fetched once at startup).</summary>
public class ChurchImagerySettings
{
    public const string SectionName = "ChurchImagery";

    public Dictionary<string, string> Queries { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
