using ChurchWebsite.Models;
using ChurchWebsite.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ChurchWebsite.Pages.Sermons;

/// <summary>Sermon detail page. USE CASE: Single sermon by id with video; 404 if not found.</summary>
public class DetailsModel : PageModel
{
    private readonly SermonService _sermonService;

    public DetailsModel(SermonService sermonService)
    {
        _sermonService = sermonService;
    }

    public Sermon? Sermon { get; set; }

    /// <summary>Loads sermon by id from SermonService; returns 404 if not found.</summary>
    public IActionResult OnGet(int id)
    {
        Sermon = _sermonService.GetById(id);
        if (Sermon == null)
            return NotFound();
        return Page();
    }
}
