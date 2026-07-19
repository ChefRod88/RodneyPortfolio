using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RodneyPortfolio.Pages;

public class PrivacyModel : PageModel
{
    public void OnGet()
    {
        ViewData["Seo"] = new RodneyPortfolio.Models.SeoMetadata
        {
            Title = "Privacy Policy | Rodney Chery",
            Description = "Privacy policy and data handling practices for Rodney Chery and RC DEV LLC.",
            CanonicalUrl = "https://www.rodneyachery.com/Privacy",
            Robots = "index, follow",
            OpenGraphImage = "https://www.rodneyachery.com/assets/images/rodney-chery-social-card.webp",
            StructuredData = new object[]
            {
                new
                {
                    @context = "https://schema.org",
                    @type = "WebPage",
                    @id = "https://www.rodneyachery.com/Privacy",
                    url = "https://www.rodneyachery.com/Privacy",
                    name = "Privacy Policy | Rodney Chery"
                }
            }
        };
    }
}

