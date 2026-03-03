using Microsoft.EntityFrameworkCore;
using RodneyPortfolio.Models;

namespace RodneyPortfolio.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ClientAccount> ClientAccounts => Set<ClientAccount>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<OtpCode> OtpCodes => Set<OtpCode>();
    public DbSet<ClientSession> ClientSessions => Set<ClientSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ClientAccount
        modelBuilder.Entity<ClientAccount>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasMaxLength(36);
            e.Property(x => x.FirstName).HasMaxLength(60).IsRequired();
            e.Property(x => x.LastName).HasMaxLength(60).IsRequired();
            e.Property(x => x.Email).HasMaxLength(200).IsRequired();
            e.Property(x => x.Phone).HasMaxLength(20).IsRequired();
            e.Property(x => x.CompanyName).HasMaxLength(200);
            e.Property(x => x.BillingAddress).HasMaxLength(300).IsRequired();
            e.Property(x => x.City).HasMaxLength(100).IsRequired();
            e.Property(x => x.State).HasMaxLength(50).IsRequired();
            e.Property(x => x.ZipCode).HasMaxLength(10).IsRequired();
            e.Property(x => x.TierInterest).HasMaxLength(20).IsRequired();
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.Ignore(x => x.FullName);
            e.Ignore(x => x.IsVerified);
            e.HasIndex(x => x.Email).IsUnique();
        });

        // Invoice
        modelBuilder.Entity<Invoice>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasMaxLength(36);
            e.Property(x => x.ClientId).HasMaxLength(36).IsRequired();
            e.Property(x => x.ClientName).HasMaxLength(200).IsRequired();
            e.Property(x => x.ClientEmail).HasMaxLength(200).IsRequired();
            e.Property(x => x.Description).HasMaxLength(500).IsRequired();
            e.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            e.Property(x => x.StripePaymentIntentId).HasMaxLength(200);
            e.Property(x => x.StripeCheckoutSessionId).HasMaxLength(200);
            e.Property(x => x.Status).HasConversion<string>();
        });

        // OtpCode
        modelBuilder.Entity<OtpCode>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasMaxLength(36);
            e.Property(x => x.Email).HasMaxLength(200).IsRequired();
            e.Property(x => x.Code).HasMaxLength(6).IsRequired();
            e.Property(x => x.Purpose).HasMaxLength(20).IsRequired();
        });

        // ClientSession
        modelBuilder.Entity<ClientSession>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasMaxLength(36);
            e.Property(x => x.ClientId).HasMaxLength(36).IsRequired();
            e.Property(x => x.Email).HasMaxLength(200).IsRequired();
            e.Ignore(x => x.Expired);
        });
    }
}