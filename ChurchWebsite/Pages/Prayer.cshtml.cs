using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ChurchWebsite.Pages;

/// <summary>Request Prayer form. USE CASE: Submit prayer requests; logged for pastoral review.</summary>
public class PrayerModel : PageModel
{
    private readonly ILogger<PrayerModel> _logger;

    public PrayerModel(ILogger<PrayerModel> logger)
    {
        _logger = logger;
    }

    [BindProperty]
    [Display(Name = "Name")]
    public string? Name { get; set; }

    [BindProperty]
    [EmailAddress]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Please share your prayer request.")]
    [Display(Name = "Prayer Request")]
    public string PrayerRequest { get; set; } = string.Empty;

    [BindProperty]
    [Display(Name = "Share anonymously")]
    public bool ShareAnonymously { get; set; }

    public bool Submitted { get; set; }  // When true, view shows thank-you message instead of form

    /// <summary>Handles form submit. Validates; logs request for pastoral review; shows thank-you.</summary>
    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        _logger.LogInformation(
            "PrayerRequest | Time: {Time} | Name: {Name} | Email: {Email} | Anonymous: {Anonymous} | Request: {Request}",
            DateTime.UtcNow.ToString("o"),
            string.IsNullOrWhiteSpace(Name) ? "(not provided)" : Name,
            string.IsNullOrWhiteSpace(Email) ? "(not provided)" : Email,
            ShareAnonymously,
            PrayerRequest
        );

        Submitted = true;
        return Page();
    }
}
