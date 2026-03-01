using SolidDry.Core.Enums;

namespace SolidDry.Core.Entities;

public class Finding
{
    public Guid Id { get; set; }
    public Guid CodeSubmissionId { get; set; }
    public CodeSubmission? CodeSubmission { get; set; }

    public QualityPrinciple Principle { get; set; }
    public string RuleKey { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Evidence { get; set; } = string.Empty;
    public FindingSeverity Severity { get; set; }
    public decimal Confidence { get; set; }
    public FindingStatus Status { get; set; } = FindingStatus.Open;
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<ReviewDecision> ReviewDecisions { get; set; } = new List<ReviewDecision>();
    public ICollection<QaAdjudication> QaAdjudications { get; set; } = new List<QaAdjudication>();
}
