using ChurchWebsite.Models;
using ChurchWebsite.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ChurchWebsite.Pages.Sermons;

public class DetailsModel : PageModel
{
    private readonly SermonService _sermonService;

    public DetailsModel(SermonService sermonService)
    {
        _sermonService = sermonService;
    }

    public Sermon? Sermon { get; set; }

    public IActionResult OnGet(int id)
    {
        Sermon = _sermonService.GetById(id);
        if (Sermon == null)
            return NotFound();
        return Page();
    }
}
