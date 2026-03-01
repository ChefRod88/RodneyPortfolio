namespace SolidDry.Core.Entities;

public class VendorProfile
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string DisplayName { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public decimal PrecisionScore { get; set; }
    public decimal ReworkRate { get; set; }
    public decimal SlaComplianceRate { get; set; }
    public int TasksReviewed { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
