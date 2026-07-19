using Microsoft.AspNetCore.Mvc;
using System.Text;
using RodneyPortfolio.Services;

namespace RodneyPortfolio.Controllers
{
    [ApiController]
    public class RssController : ControllerBase
    {
        private readonly IArticleService _articleService;

        public RssController(IArticleService articleService)
        {
            _articleService = articleService;
        }

        [HttpGet("/Articles/feed.xml")]
        [Produces("application/xml")]
        public async Task<IActionResult> GetFeedAsync()
        {
            var articles = await _articleService.GetAllArticlesAsync(includeDrafts: false);

            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>");
            sb.AppendLine("<rss version=\"2.0\" xmlns:atom=\"http://www.w3.org/2005/Atom\">");
            sb.AppendLine("  <channel>");
            sb.AppendLine("    <title>Rodney Chery | Technical Articles</title>");
            sb.AppendLine($"    <link>https://www.rodneyachery.com/Articles</link>");
            sb.AppendLine("    <description>Insights on healthcare software, AI, and ASP.NET Core.</description>");
            sb.AppendLine($"    <atom:link href=\"https://www.rodneyachery.com/Articles/feed.xml\" rel=\"self\" type=\"application/rss+xml\" />");

            foreach (var article in articles)
            {
                var articleUrl = $"https://www.rodneyachery.com/Articles/{article.Slug}";
                sb.AppendLine("    <item>");
                sb.AppendLine($"      <title>{System.Security.SecurityElement.Escape(article.Title)}</title>");
                sb.AppendLine($"      <link>{articleUrl}</link>");
                sb.AppendLine($"      <guid isPermaLink=\"true\">{articleUrl}</guid>");
                sb.AppendLine($"      <description>{System.Security.SecurityElement.Escape(article.Description)}</description>");
                sb.AppendLine($"      <pubDate>{article.DatePublished.ToString("R")}</pubDate>");
                sb.AppendLine("    </item>");
            }

            sb.AppendLine("  </channel>");
            sb.AppendLine("</rss>");

            return Content(sb.ToString(), "application/xml", Encoding.UTF8);
        }
    }
}
