namespace SolidDry.Core.Entities;

public class CodeSubmission
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public string SourceContent { get; set; } = string.Empty;
    public DateTimeOffset SubmittedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public string SubmittedBy { get; set; } = string.Empty;

    public ICollection<Finding> Findings { get; set; } = new List<Finding>();
    public ICollection<AuditEvent> AuditEvents { get; set; } = new List<AuditEvent>();
}
