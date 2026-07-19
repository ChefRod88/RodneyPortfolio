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

            var canonicalUrl = $"https://www.rodneyachery.com/Articles/{slug}";
            var defaultImageUrl = "https://www.rodneyachery.com/assets/images/rodney-chery-social-card.webp";

            ViewData["Seo"] = new SeoMetadata
            {
                Title = $"{Article.Title} | Rodney Chery",
                Description = Article.Description,
                CanonicalUrl = canonicalUrl,
                Robots = "index, follow",
                OpenGraphType = "article",
                OpenGraphImage = string.IsNullOrEmpty(Article.HeaderImageUrl) ? defaultImageUrl : Article.HeaderImageUrl,
                DatePublished = Article.DatePublished,
                DateModified = Article.DateModified,
                Keywords = Article.Tags,
                StructuredData = new object[]
                {
                    new
                    {
                        @context = "https://schema.org",
                        @type = "Article",
                        headline = Article.Title,
                        description = Article.Description,
                        image = string.IsNullOrEmpty(Article.HeaderImageUrl) ? defaultImageUrl : Article.HeaderImageUrl,
                        datePublished = Article.DatePublished.ToString("o"),
                        dateModified = (Article.DateModified ?? Article.DatePublished).ToString("o"),
                        author = new
                        {
                            @type = "Person",
                            name = "Rodney A. Chery",
                            url = "https://www.rodneyachery.com/"
                        },
                        publisher = new
                        {
                            @type = "Organization",
                            name = "RC DEV LLC",
                            logo = new
                            {
                                @type = "ImageObject",
                                url = "https://www.rodneyachery.com/favicon.svg"
                            }
                        }
                    }
                }
            };

            return Page();
        }
    }
}
