using System.ComponentModel.DataAnnotations;

namespace RodneyPortfolio.Models;

/// <summary>
/// Request body for POST /api/chat/job-match.
/// </summary>
public class JobMatchRequest
{
    [Required]
    public string JobDescription { get; set; } = string.Empty;

    public bool UseIcm { get; set; } = false;

    public int? TargetStage { get; set; }
}
