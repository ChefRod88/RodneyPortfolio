namespace RodneyPortfolio.Models;

/// <summary>
/// Response body for POST /api/chat/job-match.
/// </summary>
public class JobMatchResponse
{
    public int MatchScore { get; set; }
    public List<string> SkillsAligned { get; set; } = new();
    public List<string> Gaps { get; set; } = new();
    public List<string> TalkingPoints { get; set; } = new();
}
