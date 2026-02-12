using ChurchWebsite.Models;
using ChurchWebsite.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ChurchWebsite.Pages.Sermons;

public class IndexModel : PageModel
{
    private readonly SermonService _sermonService;

    public IndexModel(SermonService sermonService)
    {
        _sermonService = sermonService;
    }

    public List<Sermon> Sermons { get; set; } = [];

    public void OnGet()
    {
        Sermons = _sermonService.GetAll();
    }
}
