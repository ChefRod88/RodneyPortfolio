using System.IO;

namespace RodneyPortfolio.Tests;

public class ButtonWiringContractTests
{
    [Theory]
    [MemberData(nameof(ButtonAndResponsiveContractCases))]
    public void ContractTokenExists_InMarkupAndStyles(string relativePath, string caseName, string expectedToken)
    {
        var content = ReadRepoFile(relativePath);
        var exists = content.Contains(expectedToken, StringComparison.Ordinal);
        Assert.True(exists, $"Contract case '{caseName}' missing token '{expectedToken}' in '{relativePath}'.");
    }

    [Fact]
    public void SharedLayout_NavigationLinks_UseHomepageAnchorsAcrossPages()
    {
        var layout = ReadRepoFile("Pages/Shared/_Layout.cshtml");

        Assert.DoesNotContain("href=\"#about\"", layout, StringComparison.Ordinal);
        Assert.DoesNotContain("href=\"#services\"", layout, StringComparison.Ordinal);
        Assert.DoesNotContain("href=\"#capabilities\"", layout, StringComparison.Ordinal);
        Assert.DoesNotContain("href=\"#pricing\"", layout, StringComparison.Ordinal);
        Assert.DoesNotContain("href=\"#testimonials\"", layout, StringComparison.Ordinal);
        Assert.DoesNotContain("href=\"#quote\"", layout, StringComparison.Ordinal);
    }

    [Fact]
    public void ContractCaseCount_IsExactly100()
    {
        Assert.Equal(100, ButtonAndResponsiveContractCases().Count());
    }

    public static IEnumerable<object[]> ButtonAndResponsiveContractCases()
    {
        var cases = new (string RelativePath, string CaseName, string ExpectedToken)[]
        {
            // Layout nav/footer wiring (20)
            ("Pages/Shared/_Layout.cshtml", "layout-nav-about", "href=\"/#about\""),
            ("Pages/Shared/_Layout.cshtml", "layout-nav-services", "href=\"/#services\""),
            ("Pages/Shared/_Layout.cshtml", "layout-nav-work", "href=\"/#capabilities\""),
            ("Pages/Shared/_Layout.cshtml", "layout-nav-pricing", "href=\"/#pricing\""),
            ("Pages/Shared/_Layout.cshtml", "layout-nav-reviews", "href=\"/#testimonials\""),
            ("Pages/Shared/_Layout.cshtml", "layout-nav-faq-link", "href=\"/Faq\""),
            ("Pages/Shared/_Layout.cshtml", "layout-nav-faq-class", "class=\"rc-nav-faq\""),
            ("Pages/Shared/_Layout.cshtml", "layout-nav-quote-cta", "href=\"/#quote\"        onclick=\"toggleMenu()\" class=\"rc-nav-cta\""),
            ("Pages/Shared/_Layout.cshtml", "layout-footer-about", "<a href=\"/#about\">About</a>"),
            ("Pages/Shared/_Layout.cshtml", "layout-footer-services", "<a href=\"/#services\">Services</a>"),
            ("Pages/Shared/_Layout.cshtml", "layout-footer-work", "<a href=\"/#capabilities\">Work</a>"),
            ("Pages/Shared/_Layout.cshtml", "layout-footer-pricing", "<a href=\"/#pricing\">Pricing</a>"),
            ("Pages/Shared/_Layout.cshtml", "layout-footer-quote", "<a href=\"/#quote\">Quote</a>"),
            ("Pages/Shared/_Layout.cshtml", "layout-footer-contact", "<a href=\"/#quote\">Contact</a>"),
            ("Pages/Shared/_Layout.cshtml", "layout-togglemenu-function", "function toggleMenu()"),
            ("Pages/Shared/_Layout.cshtml", "layout-togglemenu-links", "document.querySelector('.menu-links').classList.toggle('open');"),
            ("Pages/Shared/_Layout.cshtml", "layout-togglemenu-icon", "document.querySelector('.hamburger-icon').classList.toggle('open');"),
            ("Pages/Shared/_Layout.cshtml", "layout-scroll-reveal-observer", "document.querySelectorAll('.rc-fade').forEach(el => rcObserver.observe(el));"),
            ("Pages/Shared/_Layout.cshtml", "layout-faq-accordion-query", "document.querySelectorAll(\".faq-q\").forEach(btn =>"),
            ("Pages/Shared/_Layout.cshtml", "layout-faq-accordion-close", "b.nextElementSibling?.classList.remove(\"open\");"),

            // Index page button and action wiring (45)
            ("Pages/Index.cshtml", "index-hero-quote-button", "onclick=\"location.href='#quote'\">Get a Quote</button>"),
            ("Pages/Index.cshtml", "index-social-linkedin", "onclick=\"location.href='https://www.linkedin.com/in/rodneyachery/'\""),
            ("Pages/Index.cshtml", "index-social-github", "onclick=\"window.open('https://github.com/ChefRod88/RodneyPortfolio', '_blank')\""),
            ("Pages/Index.cshtml", "index-arrow-services", "onclick=\"location.href='#services'\""),
            ("Pages/Index.cshtml", "index-arrow-capabilities", "onclick=\"location.href='#capabilities'\""),
            ("Pages/Index.cshtml", "index-arrow-process", "onclick=\"location.href='#process'\""),
            ("Pages/Index.cshtml", "index-cap-demo-link", "<a href=\"#ask-rodney\" class=\"rc-cap-link\">Try the Demo →</a>"),
            ("Pages/Index.cshtml", "index-chat-send-button-id", "id=\"chat-send\""),
            ("Pages/Index.cshtml", "index-chat-send-button-class", "class=\"chat-send-btn\""),
            ("Pages/Index.cshtml", "index-job-match-analyze-id", "id=\"job-match-analyze\""),
            ("Pages/Index.cshtml", "index-job-match-analyze-class", "class=\"chat-job-match-btn\""),
            ("Pages/Index.cshtml", "index-pricing-get-started", "onclick=\"location.href='#quote'\" style=\"width:100%;\">Get Started</button>"),
            ("Pages/Index.cshtml", "index-pricing-book-call", "onclick=\"location.href='#quote'\" style=\"width:100%;\">Book a Call</button>"),
            ("Pages/Index.cshtml", "index-quote-submit-button", "class=\"btn btn-outline-dark rc-quote-submit\">Submit Request"),
            ("Pages/Index.cshtml", "index-quote-form-id", "id=\"quoteForm\""),
            ("Pages/Index.cshtml", "index-quote-form-handler", "onsubmit=\"handleQuoteSubmit(event)\""),
            ("Pages/Index.cshtml", "index-quote-error-box", "id=\"quoteError\""),
            ("Pages/Index.cshtml", "index-quote-success-box", "id=\"quoteSuccess\""),
            ("Pages/Index.cshtml", "index-contact-section-id", "id=\"contact\" class=\"contact-section rc-fade\""),
            ("Pages/Index.cshtml", "index-contact-email-link", "href=\"mailto:chefrodneyachery@gmail.com\" class=\"contact-item\""),
            ("Pages/Index.cshtml", "index-contact-linkedin-link", "href=\"https://www.linkedin.com/in/rodneyachery/\" target=\"_blank\" rel=\"noopener noreferrer\" class=\"contact-item\""),
            ("Pages/Index.cshtml", "index-chat-transparency-toggle", "id=\"chat-transparency-toggle\""),
            ("Pages/Index.cshtml", "index-chat-transparency-controls", "aria-controls=\"chat-transparency-content\""),
            ("Pages/Index.cshtml", "index-chat-transparency-content", "id=\"chat-transparency-content\" class=\"chat-transparency-content\" hidden"),
            ("Pages/Index.cshtml", "index-job-match-input", "id=\"job-match-input\" class=\"chat-job-match-input\""),
            ("Pages/Index.cshtml", "index-chat-input", "id=\"chat-input\" class=\"chat-input\""),
            ("Pages/Index.cshtml", "index-chat-messages-log", "id=\"chat-messages\" class=\"chat-messages\""),
            ("Pages/Index.cshtml", "index-doc-button-implementation", "class=\"chat-docs-btn\">View Documentation</a>"),
            ("Pages/Index.cshtml", "index-doc-button-api-key", "class=\"chat-docs-btn\">API Key Docs</a>"),
            ("Pages/Index.cshtml", "index-doc-button-feature-docs", "class=\"chat-docs-btn\">Feature Technical Docs</a>"),
            ("Pages/Index.cshtml", "index-quote-section-anchor", "<section id=\"quote\" class=\"rc-fade\">"),
            ("Pages/Index.cshtml", "index-contact-section-anchor", "<section id=\"contact\" class=\"contact-section rc-fade\">"),
            ("Pages/Index.cshtml", "index-socials-container", "id=\"socials-container\""),
            ("Pages/Index.cshtml", "index-button-container", "class=\"btn-container\""),
            ("Pages/Index.cshtml", "index-quote-submit-fetch", "fetch(form.action, {"),
            ("Pages/Index.cshtml", "index-quote-submit-method", "method: 'POST',"),
            ("Pages/Index.cshtml", "index-quote-submit-header", "headers: { 'X-Requested-With': 'XMLHttpRequest' }"),
            ("Pages/Index.cshtml", "index-quote-submit-hide-fields", "formFields.forEach(el => el.style.display = 'none');"),
            ("Pages/Index.cshtml", "index-quote-submit-success", "document.getElementById('quoteSuccess').style.display = 'block';"),
            ("Pages/Index.cshtml", "index-quote-submit-disable", "submitBtn.disabled = true;"),
            ("Pages/Index.cshtml", "index-quote-submit-enable-on-error", "submitBtn.disabled = false;"),
            ("Pages/Index.cshtml", "index-quote-submit-error-show", "errorBox.style.display = 'block';"),
            ("Pages/Index.cshtml", "index-quote-submit-error-hide", "errorBox.style.display = 'none';"),
            ("Pages/Index.cshtml", "index-google-review-link", "<a href=\"https://g.page/r/YOUR_GOOGLE_REVIEW_LINK\" target=\"_blank\""),
            ("Pages/Index.cshtml", "index-google-review-button-class", "class=\"btn btn-outline-dark\">"),

            // FAQ page button/link wiring + local responsiveness (20)
            ("Pages/Faq.cshtml", "faq-page-directive", "@page"),
            ("Pages/Faq.cshtml", "faq-title", "ViewData[\"Title\"] = \"FAQ & Service Standards\";"),
            ("Pages/Faq.cshtml", "faq-inline-quote-link", "<a href=\"/#quote\" class=\"faq-link\">Quote Request form</a>"),
            ("Pages/Faq.cshtml", "faq-cta-quote-link", "<a href=\"/#quote\" class=\"btn btn-outline-dark\">Request a Quote</a>"),
            ("Pages/Faq.cshtml", "faq-cta-email-link", "<a href=\"mailto:chefrodneyachery@gmail.com\" class=\"btn btn-outline-dark faq-btn-ghost\">Email Directly</a>"),
            ("Pages/Faq.cshtml", "faq-quicknav-container", "<div class=\"faq-quicknav-links\">"),
            ("Pages/Faq.cshtml", "faq-quicknav-general", "<a href=\"#general\">General</a>"),
            ("Pages/Faq.cshtml", "faq-quicknav-sla", "<a href=\"#sla\">SLA</a>"),
            ("Pages/Faq.cshtml", "faq-quicknav-support", "<a href=\"#support\">Support</a>"),
            ("Pages/Faq.cshtml", "faq-quicknav-hosting", "<a href=\"#hosting\">Hosting</a>"),
            ("Pages/Faq.cshtml", "faq-quicknav-newwork", "<a href=\"#newwork\">New Work</a>"),
            ("Pages/Faq.cshtml", "faq-question-button", "class=\"faq-q\" aria-expanded=\"false\""),
            ("Pages/Faq.cshtml", "faq-answer-container", "class=\"faq-a\""),
            ("Pages/Faq.cshtml", "faq-answer-open-style", ".faq-a.open { display: block; }"),
            ("Pages/Faq.cshtml", "faq-chevron-rotation-style", ".faq-q[aria-expanded=\"true\"] .faq-chevron { transform: rotate(90deg); }"),
            ("Pages/Faq.cshtml", "faq-cta-buttons-style", ".faq-cta-btns { display: flex;"),
            ("Pages/Faq.cshtml", "faq-mobile-media-query-escaped", "@@media (max-width: 768px) {"),
            ("Pages/Faq.cshtml", "faq-mobile-response-grid", ".faq-response-grid { grid-template-columns: 1fr; }"),
            ("Pages/Faq.cshtml", "faq-mobile-tier-desc-hidden", ".faq-tier-row .faq-tier-desc { display: none; }"),
            ("Pages/Faq.cshtml", "faq-mobile-quicknav-stack", ".faq-quicknav { flex-direction: column; gap: 0.8rem; }"),

            // Global CSS responsiveness and button styles (15)
            ("wwwroot/css/site.css", "css-mobile-breakpoint-768", "@media (max-width: 768px) {"),
            ("wwwroot/css/site.css", "css-mobile-show-hamburger", "#hamburger-nav .hamburger-icon { display: flex; }"),
            ("wwwroot/css/site.css", "css-mobile-open-menu", "#hamburger-nav .menu-links.open { display: block; }"),
            ("wwwroot/css/site.css", "css-mobile-contact-stack", ".contact-card { flex-direction: column; gap: 1rem; }"),
            ("wwwroot/css/site.css", "css-mobile-footer-wrap", ".rc-footer-links { gap: 1.2rem; flex-wrap: wrap; justify-content: center; }"),
            ("wwwroot/css/site.css", "css-tablet-breakpoint-900", "@media (max-width: 900px) {"),
            ("wwwroot/css/site.css", "css-tablet-quote-grid", ".rc-quote-grid { grid-template-columns: 1fr; }"),
            ("wwwroot/css/site.css", "css-tablet-pricing-grid", ".rc-pricing-grid { grid-template-columns: 1fr; }"),
            ("wwwroot/css/site.css", "css-tablet-services-grid", ".rc-services-grid { grid-template-columns: 1fr 1fr; }"),
            ("wwwroot/css/site.css", "css-mobile-services-grid", ".rc-services-grid { grid-template-columns: 1fr; }"),
            ("wwwroot/css/site.css", "css-mobile-form-row-grid", ".rc-form-row { grid-template-columns: 1fr; }"),
            ("wwwroot/css/site.css", "css-button-base-style", ".btn, button.btn {"),
            ("wwwroot/css/site.css", "css-nav-quote-style", ".rc-nav-cta {"),
            ("wwwroot/css/site.css", "css-nav-faq-style", ".rc-nav-faq {"),
            ("wwwroot/css/site.css", "css-nav-faq-hover", ".rc-nav-faq:hover {"),
        };

        foreach (var contract in cases)
        {
            yield return new object[] { contract.RelativePath, contract.CaseName, contract.ExpectedToken };
        }
    }

    private static string ReadRepoFile(string relativePath)
    {
        var root = FindRepoRoot();
        var fullPath = Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));
        return File.ReadAllText(fullPath);
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "RodneyPortfolio.sln")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException("Could not locate repository root containing RodneyPortfolio.sln.");
    }
}
