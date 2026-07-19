using Microsoft.AspNetCore.Mvc.RazorPages;
using RodneyPortfolio.Models;
using RodneyPortfolio.Services;

namespace RodneyPortfolio.Pages.Insights;

public class IndexModel : PageModel
{
    private readonly IMarkdownContentService _contentService;
    public List<Article> Articles { get; set; } = new();

    public IndexModel(IMarkdownContentService contentService)
    {
        _contentService = contentService;
    }

    public async Task OnGetAsync()
    {
        Articles = await _contentService.GetAllArticlesAsync("Content/Insights");
    }
}
