using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ChurchWebsite.Pages;

/// <summary>Request Prayer form. USE CASE: Submit prayer requests (currently in-memory, no persistence).</summary>
public class PrayerModel : PageModel
{
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

    /// <summary>Handles form submit. Validates; if valid, sets Submitted and shows thank-you. No persistence yet.</summary>
    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        Submitted = true;
        return Page();
    }
}
