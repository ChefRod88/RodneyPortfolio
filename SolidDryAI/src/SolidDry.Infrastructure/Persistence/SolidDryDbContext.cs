using Microsoft.EntityFrameworkCore;
using SolidDry.Core.Entities;

namespace SolidDry.Infrastructure.Persistence;

public class SolidDryDbContext : DbContext
{
    public SolidDryDbContext(DbContextOptions<SolidDryDbContext> options) : base(options)
    {
    }

    public DbSet<CodeSubmission> CodeSubmissions => Set<CodeSubmission>();
    public DbSet<Finding> Findings => Set<Finding>();
    public DbSet<ReviewDecision> ReviewDecisions => Set<ReviewDecision>();
    public DbSet<QaAdjudication> QaAdjudications => Set<QaAdjudication>();
    public DbSet<VendorProfile> VendorProfiles => Set<VendorProfile>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CodeSubmission>()
            .HasMany(s => s.Findings)
            .WithOne(f => f.CodeSubmission)
            .HasForeignKey(f => f.CodeSubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CodeSubmission>()
            .HasMany(s => s.AuditEvents)
            .WithOne(a => a.CodeSubmission)
            .HasForeignKey(a => a.CodeSubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Finding>()
            .HasMany(f => f.ReviewDecisions)
            .WithOne(r => r.Finding)
            .HasForeignKey(r => r.FindingId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Finding>()
            .HasMany(f => f.QaAdjudications)
            .WithOne(q => q.Finding)
            .HasForeignKey(q => q.FindingId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Finding>()
            .Property(f => f.Confidence)
            .HasPrecision(5, 4);

        modelBuilder.Entity<VendorProfile>()
            .Property(v => v.PrecisionScore)
            .HasPrecision(5, 4);

        modelBuilder.Entity<VendorProfile>()
            .Property(v => v.ReworkRate)
            .HasPrecision(5, 4);

        modelBuilder.Entity<VendorProfile>()
            .Property(v => v.SlaComplianceRate)
            .HasPrecision(5, 4);
    }
}
