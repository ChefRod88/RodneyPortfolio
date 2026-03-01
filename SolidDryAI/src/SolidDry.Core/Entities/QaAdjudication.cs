using SolidDry.Core.Enums;

namespace SolidDry.Core.Entities;

public class QaAdjudication
{
    public Guid Id { get; set; }
    public Guid FindingId { get; set; }
    public Finding? Finding { get; set; }

    public string QaReviewerId { get; set; } = string.Empty;
    public ReviewLabel FinalLabel { get; set; }
    public string DecisionNote { get; set; } = string.Empty;
    public string CalibrationTag { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
