using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RodneyPortfolio.Pages;

public class FaqModel : PageModel
{
    private readonly ILogger<FaqModel> _logger;

    public FaqModel(ILogger<FaqModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
        ViewData["Seo"] = new RodneyPortfolio.Models.SeoMetadata
        {
            Title = "FAQ & Standards | Rodney Chery",
            Description = "Frequently asked questions about my development process, engineering standards, and consulting services.",
            CanonicalUrl = "https://www.rodneyachery.com/Faq",
            Robots = "index, follow",
            OpenGraphImage = "https://www.rodneyachery.com/assets/images/rodney-chery-social-card.webp",
            StructuredData = new object[]
            {
                new
                {
                    @context = "https://schema.org",
                    @type = "WebPage",
                    @id = "https://www.rodneyachery.com/Faq",
                    url = "https://www.rodneyachery.com/Faq",
                    name = "FAQ & Standards | Rodney Chery"
                }
            }
        };
    }
}