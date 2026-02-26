using ChurchWebsite.Models;
using ChurchWebsite.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ChurchWebsite.Pages.Sermons;

/// <summary>Sermons list page. USE CASE: Display all sermons from SermonService.</summary>
public class IndexModel : PageModel
{
    private readonly ISermonService _sermonService;

    public IndexModel(ISermonService sermonService)
    {
        _sermonService = sermonService;
    }

    public List<Sermon> Sermons { get; set; } = [];

    public void OnGet()
    {
        Sermons = _sermonService.GetAll();
    }
}
