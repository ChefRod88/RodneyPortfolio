using SolidDry.Core.Enums;

namespace SolidDry.Core.Entities;

public class ReviewDecision
{
    public Guid Id { get; set; }
    public Guid FindingId { get; set; }
    public Finding? Finding { get; set; }

    public string ReviewerId { get; set; } = string.Empty;
    public string? VendorId { get; set; }
    public ReviewLabel Label { get; set; }
    public string Rationale { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
