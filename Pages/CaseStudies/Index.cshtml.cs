using Microsoft.AspNetCore.Mvc.RazorPages;
using RodneyPortfolio.Models;
using RodneyPortfolio.Services;

namespace RodneyPortfolio.Pages.CaseStudies;

public class IndexModel : PageModel
{
    private readonly IMarkdownContentService _contentService;
    public List<Article> CaseStudies { get; set; } = new();

    public IndexModel(IMarkdownContentService contentService)
    {
        _contentService = contentService;
    }

    public async Task OnGetAsync()
    {
        CaseStudies = await _contentService.GetAllArticlesAsync("Content/CaseStudies");
    }
}
