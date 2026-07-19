using Microsoft.AspNetCore.Mvc;
using System.Text;
using RodneyPortfolio.Services;

namespace RodneyPortfolio.Controllers
{
    [ApiController]
    public class SeoController : ControllerBase
    {
        private readonly ICanonicalUrlService _canonicalUrlService;
        private readonly IWebHostEnvironment _env;
        private readonly IArticleService _articleService;

        public SeoController(ICanonicalUrlService canonicalUrlService, IWebHostEnvironment env, IArticleService articleService)
        {
            _canonicalUrlService = canonicalUrlService;
            _env = env;
            _articleService = articleService;
        }

        [HttpGet("/robots.txt")]
        [Produces("text/plain")]
        public IActionResult RobotsTxt()
        {
            var sb = new StringBuilder();
            
            if (_env.IsDevelopment())
            {
                sb.AppendLine("User-agent: *");
                sb.AppendLine("Disallow: /");
            }
            else
            {
                sb.AppendLine("User-agent: *");
                sb.AppendLine("Allow: /");
                sb.AppendLine();
                sb.AppendLine("Disallow: /Admin/");
                sb.AppendLine("Disallow: /Portal/");
                sb.AppendLine("Disallow: /api/");
                sb.AppendLine("Disallow: /Agreement");
                sb.AppendLine("Disallow: /Support");
                sb.AppendLine();
                sb.AppendLine($"Sitemap: {_canonicalUrlService.GetCanonicalUrl(HttpContext, "/sitemap.xml")}");
            }
            
            return Content(sb.ToString(), "text/plain", Encoding.UTF8);
        }

        [HttpGet("/sitemap.xml")]
        [Produces("application/xml")]
        public async Task<IActionResult> SitemapXmlAsync()
        {
            // Note: Ideally, this would dynamically list all public routes and published articles.
            var routes = new[] {
                "/",
                "/Projects",
                "/Faq",
                "/Privacy",
                "/Articles"
            };

            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");
            
            var lastMod = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:sszzz");

            foreach (var route in routes)
            {
                sb.AppendLine("  <url>");
                sb.AppendLine($"    <loc>{_canonicalUrlService.GetCanonicalUrl(HttpContext, route)}</loc>");
                sb.AppendLine($"    <lastmod>{lastMod}</lastmod>");
                sb.AppendLine("  </url>");
            }
            
            var articles = await _articleService.GetAllArticlesAsync(includeDrafts: false);
            foreach (var article in articles)
            {
                sb.AppendLine("  <url>");
                sb.AppendLine($"    <loc>{_canonicalUrlService.GetCanonicalUrl(HttpContext, $"/Articles/{article.Slug}")}</loc>");
                sb.AppendLine($"    <lastmod>{(article.DateModified ?? article.DatePublished).ToString("yyyy-MM-ddTHH:mm:sszzz")}</lastmod>");
                sb.AppendLine("  </url>");
            }
            
            sb.AppendLine("</urlset>");
            
            return Content(sb.ToString(), "application/xml", Encoding.UTF8);
        }
    }
}
