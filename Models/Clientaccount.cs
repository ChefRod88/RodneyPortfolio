using System.ComponentModel.DataAnnotations;

namespace RodneyPortfolio.Models;

public class ClientAccount
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string BillingAddress { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string TierInterest { get; set; } = "Starter";
    public string Status { get; set; } = "Pending"; // Pending | Active | Suspended
    public DateTimeOffset RegisteredAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? VerifiedAt { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }
    public bool IsVerified => VerifiedAt.HasValue;
}

public class OtpCode
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty; // "register" | "login"
    public DateTimeOffset ExpiresAt { get; set; }
    public bool Used { get; set; } = false;
}

public class ClientSession
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ClientId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ExpiresAt { get; set; } = DateTimeOffset.UtcNow.AddHours(24);
    public bool Expired => DateTimeOffset.UtcNow > ExpiresAt;
}

public class RegisterInput
{
    [Required, StringLength(60)] public string FirstName { get; set; } = string.Empty;
    [Required, StringLength(60)] public string LastName { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(200)] public string Email { get; set; } = string.Empty;
    [Required, Phone, StringLength(20)] public string Phone { get; set; } = string.Empty;
    [StringLength(200)] public string? CompanyName { get; set; }
    [Required, StringLength(300)] public string BillingAddress { get; set; } = string.Empty;
    [Required, StringLength(100)] public string City { get; set; } = string.Empty;
    [Required, StringLength(50)] public string State { get; set; } = string.Empty;
    [Required, StringLength(10)] public string ZipCode { get; set; } = string.Empty;
    [Required] public string TierInterest { get; set; } = "Starter";
}

public class OtpVerifyInput
{
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required, StringLength(6, MinimumLength = 6)] public string Code { get; set; } = string.Empty;
    public string Purpose { get; set; } = "login";
}

public class LoginInput
{
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
}

public class SupportMessageInput
{
    [Required, StringLength(120)] public string Subject { get; set; } = string.Empty;
    [Required, StringLength(2000)] public string Message { get; set; } = string.Empty;
}