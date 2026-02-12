using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ChurchWebsite.Pages;

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

    public bool Submitted { get; set; }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        Submitted = true;
        return Page();
    }
}
