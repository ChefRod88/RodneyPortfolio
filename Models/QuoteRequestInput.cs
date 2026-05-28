using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace RodneyPortfolio.Models;

public class QuoteRequestInput
{
    [FromForm(Name = "g-recaptcha-response")]
    public string? RecaptchaToken { get; set; }

    [FromForm(Name = "Website")]
    public string? Website { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Company { get; set; }

    [Required, StringLength(120)]
    public string ServiceNeeded { get; set; } = string.Empty;

    [Required, StringLength(120)]
    public string EstimatedBudget { get; set; } = string.Empty;

    [Required, StringLength(4000)]
    public string ProjectDescription { get; set; } = string.Empty;

    [StringLength(120)]
    public string? Timeline { get; set; }
}
