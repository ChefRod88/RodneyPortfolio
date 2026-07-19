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

            ViewData["Seo"] = new SeoMetadata
            {
                Title = "Technical Articles | Rodney Chery",
                Description = "Insights on healthcare software, AI, and ASP.NET Core.",
                CanonicalUrl = "https://www.rodneyachery.com/Articles",
                Robots = "index, follow",
                OpenGraphImage = "https://www.rodneyachery.com/assets/images/rodney-chery-social-card.webp",
                StructuredData = new object[]
                {
                    new
                    {
                        @context = "https://schema.org",
                        @type = "Blog",
                        @id = "https://www.rodneyachery.com/Articles",
                        url = "https://www.rodneyachery.com/Articles",
                        name = "Technical Articles | Rodney Chery"
                    }
                }
            };
        }
    }
}
