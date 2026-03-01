namespace SolidDry.Core.Entities;

public class AuditEvent
{
    public Guid Id { get; set; }
    public Guid? CodeSubmissionId { get; set; }
    public CodeSubmission? CodeSubmission { get; set; }
    public Guid? FindingId { get; set; }

    public string EventType { get; set; } = string.Empty;
    public string ActorId { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
