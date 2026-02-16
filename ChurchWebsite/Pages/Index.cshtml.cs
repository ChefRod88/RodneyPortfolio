using ChurchWebsite.Models;
using ChurchWebsite.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace ChurchWebsite.Pages;

/// <summary>Home page. USE CASE: Hero, cards, mission, ministries, latest sermons.</summary>
public class IndexModel : PageModel
{
    private readonly ChurchSettings _church;
    private readonly SermonService _sermonService;

    public IndexModel(IOptions<ChurchSettings> churchOptions, SermonService sermonService)
    {
        _church = churchOptions.Value;
        _sermonService = sermonService;
    }

    public ChurchSettings Church => _church;       // Exposed to view for hero, mission, etc.
    public List<Sermon> LatestSermons { get; set; } = [];  // Top 3 sermons for home page

    /// <summary>Loads latest 3 sermons from SermonService for home page display</summary>
    public void OnGet()
    {
        LatestSermons = _sermonService.GetAll().Take(3).ToList();
    }
}
