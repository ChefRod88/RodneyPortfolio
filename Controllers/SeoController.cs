using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Xml.Linq;

namespace RodneyPortfolio.Controllers;

public class SeoController : Controller
{
    [Route("robots.txt")]
    [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any)]
    public IActionResult RobotsTxt()
    {
        var sb = new StringBuilder();
        sb.AppendLine("User-agent: *");
        sb.AppendLine("Allow: /");
        sb.AppendLine("Disallow: /Admin/");
        sb.AppendLine("Disallow: /Portal/");
        sb.AppendLine("Disallow: /Agreement");
        sb.AppendLine();
        sb.AppendLine($"Sitemap: https://{Request.Host}/sitemap.xml");

        return Content(sb.ToString(), "text/plain", Encoding.UTF8);
    }

    [Route("sitemap.xml")]
    [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any)]
    public IActionResult SitemapXml()
    {
        var host = $"https://{Request.Host}";
        var lastMod = DateTime.UtcNow.ToString("yyyy-MM-dd");

        XNamespace xmlns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        var sitemap = new XElement(xmlns + "urlset",
            CreateUrlElement(xmlns, $"{host}/", lastMod, "1.0", "weekly"),
            CreateUrlElement(xmlns, $"{host}/Projects", lastMod, "0.9", "monthly"),
            CreateUrlElement(xmlns, $"{host}/Faq", lastMod, "0.8", "monthly"),
            CreateUrlElement(xmlns, $"{host}/Support", lastMod, "0.7", "yearly")
        );

        var xml = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), sitemap);
        return Content(xml.ToString(), "application/xml", Encoding.UTF8);
    }

    private XElement CreateUrlElement(XNamespace xmlns, string url, string lastMod, string priority, string changeFreq)
    {
        return new XElement(xmlns + "url",
            new XElement(xmlns + "loc", url),
            new XElement(xmlns + "lastmod", lastMod),
            new XElement(xmlns + "changefreq", changeFreq),
            new XElement(xmlns + "priority", priority)
        );
    }
}
