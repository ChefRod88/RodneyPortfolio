using Microsoft.AspNetCore.Mvc.RazorPages;
using RodneyPortfolio.Services;
using RodneyPortfolio.Models;

namespace RodneyPortfolio.Pages.Articles
{
    public class IndexModel : PageModel
    {
        private readonly IArticleService _articleService;

        public IndexModel(IArticleService articleService)
        {
            _articleService = articleService;
        }

        public List<Article> Articles { get; set; } = new();

        public async Task OnGetAsync()
        {
            var isDevelopment = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment();
            Articles = await _articleService.GetAllArticlesAsync(includeDrafts: isDevelopment);

            ViewData["Title"] = "Technical Articles | Rodney Chery";
        }
    }
}
