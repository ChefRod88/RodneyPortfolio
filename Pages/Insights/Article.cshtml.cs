using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RodneyPortfolio.Models;
using RodneyPortfolio.Services;

namespace RodneyPortfolio.Pages.Insights;

public class ArticleModel : PageModel
{
    private readonly IMarkdownContentService _contentService;
    public Article? Article { get; set; }

    public ArticleModel(IMarkdownContentService contentService)
    {
        _contentService = contentService;
    }

    public async Task<IActionResult> OnGetAsync(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug)) return RedirectToPage("/Insights/Index");

        Article = await _contentService.GetArticleAsync("Content/Insights", slug);
        if (Article == null) return NotFound();

        return Page();
    }
}
