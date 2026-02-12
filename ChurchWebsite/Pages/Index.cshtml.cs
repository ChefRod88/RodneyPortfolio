using ChurchWebsite.Models;
using ChurchWebsite.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace ChurchWebsite.Pages;

public class IndexModel : PageModel
{
    private readonly ChurchSettings _church;
    private readonly SermonService _sermonService;

    public IndexModel(IOptions<ChurchSettings> churchOptions, SermonService sermonService)
    {
        _church = churchOptions.Value;
        _sermonService = sermonService;
    }

    public ChurchSettings Church => _church;
    public List<Sermon> LatestSermons { get; set; } = [];

    public void OnGet()
    {
        LatestSermons = _sermonService.GetAll().Take(3).ToList();
    }
}
