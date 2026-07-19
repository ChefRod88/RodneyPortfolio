using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RodneyPortfolio.Services;
using RodneyPortfolio.Models;

namespace RodneyPortfolio.Pages.Articles
{
    public class DetailModel : PageModel
    {
        private readonly IArticleService _articleService;
        private readonly IWebHostEnvironment _env;

        public DetailModel(IArticleService articleService, IWebHostEnvironment env)
        {
            _articleService = articleService;
            _env = env;
        }

        public Article? Article { get; set; }

        public async Task<IActionResult> OnGetAsync(string slug)
        {
            Article = await _articleService.GetArticleBySlugAsync(slug);

            if (Article == null || (Article.IsDraft && !_env.IsDevelopment()))
            {
                return NotFound();
            }

            ViewData["Title"] = $"{Article.Title} | Rodney Chery";

            return Page();
        }
    }
}
