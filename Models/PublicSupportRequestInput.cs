using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace RodneyPortfolio.Models;

public class PublicSupportRequestInput
{
    [FromForm(Name = "g-recaptcha-response")]
    public string? RecaptchaToken { get; set; }

    [FromForm(Name = "Website")]
    public string? Website { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [StringLength(500)]
    public string? SiteOrProject { get; set; }

    [Required, StringLength(120)]
    public string Subject { get; set; } = string.Empty;

    [Required, StringLength(2000)]
    public string Message { get; set; } = string.Empty;
}
