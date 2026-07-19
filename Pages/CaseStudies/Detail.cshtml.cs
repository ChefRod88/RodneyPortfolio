using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RodneyPortfolio.Models;
using RodneyPortfolio.Services;

namespace RodneyPortfolio.Pages.CaseStudies;

public class DetailModel : PageModel
{
    private readonly IMarkdownContentService _contentService;
    public Article? CaseStudy { get; set; }

    public DetailModel(IMarkdownContentService contentService)
    {
        _contentService = contentService;
    }

    public async Task<IActionResult> OnGetAsync(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug)) return RedirectToPage("/CaseStudies/Index");

        CaseStudy = await _contentService.GetArticleAsync("Content/CaseStudies", slug);
        if (CaseStudy == null) return NotFound();

        return Page();
    }
}
