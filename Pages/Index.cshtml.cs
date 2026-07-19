using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using RodneyPortfolio.Models;
using RodneyPortfolio.Services;

namespace RodneyPortfolio.Pages;

public class IndexModel : PageModel
{
    private readonly IQuoteSubmissionService _quoteSubmissionService;
    private readonly IRecaptchaVerificationService _recaptchaVerificationService;
    private readonly RecaptchaOptions _recaptchaOptions;
    private readonly ILogger<IndexModel> _logger;

    public string RecaptchaSiteKey => _recaptchaOptions.SiteKey;

    public IndexModel(
        IQuoteSubmissionService quoteSubmissionService,
        IRecaptchaVerificationService recaptchaVerificationService,
        IOptions<RecaptchaOptions> recaptchaOptions,
        ILogger<IndexModel> logger)
    {
        _quoteSubmissionService = quoteSubmissionService;
        _recaptchaVerificationService = recaptchaVerificationService;
        _recaptchaOptions = recaptchaOptions.Value;
        _logger = logger;
    }

    public void OnGet()
    {
        ViewData["Seo"] = new SeoMetadata
        {
            Title = "Rodney Chery | Healthcare Software & .NET Engineering",
            Description = "Rodney Chery develops secure healthcare applications, patient and claimant portals, AI-assisted workflows, ASP.NET Core systems, APIs, and cloud deployment solutions through RC DEV LLC.",
            CanonicalUrl = "https://www.rodneyachery.com/",
            Robots = "index, follow",
            OpenGraphImage = "https://www.rodneyachery.com/assets/images/rodney-chery-social-card.webp",
            StructuredData = new object[]
            {
                new
                {
                    @context = "https://schema.org",
                    @type = "WebSite",
                    @id = "https://www.rodneyachery.com/#website",
                    url = "https://www.rodneyachery.com/",
                    name = "Rodney Chery",
                    publisher = new { @id = "https://www.rodneyachery.com/#rc-dev-llc" }
                },
                new
                {
                    @context = "https://schema.org",
                    @type = "Person",
                    @id = "https://www.rodneyachery.com/#rodney-chery",
                    name = "Rodney A. Chery",
                    url = "https://www.rodneyachery.com/",
                    jobTitle = "Software Developer",
                    sameAs = new[] { "https://www.linkedin.com/in/rodneyachery/", "https://github.com/ChefRod88" },
                    mainEntityOfPage = new { @id = "https://www.rodneyachery.com/#website" }
                },
                new
                {
                    @context = "https://schema.org",
                    @type = "Organization",
                    @id = "https://www.rodneyachery.com/#rc-dev-llc",
                    name = "RC DEV LLC",
                    url = "https://www.rodneyachery.com/",
                    founder = new { @id = "https://www.rodneyachery.com/#rodney-chery" }
                }
            }
        };
    }

    [EnableRateLimiting("QuotePolicy")]
    public async Task<IActionResult> OnPostQuoteAsync([FromForm] QuoteRequestInput request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.Website))
        {
            _logger.LogWarning("Quote request blocked by honeypot from {IpAddress}", HttpContext.Connection.RemoteIpAddress);
            return BadRequest(new { ok = false, message = "Unable to submit request." });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(new { ok = false, message = "Please complete all required fields." });
        }

        var verifyResult = await _recaptchaVerificationService.VerifyAsync(
            request.RecaptchaToken ?? string.Empty,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            cancellationToken);
        if (!verifyResult.IsSuccess)
        {
            _logger.LogWarning(
                "Quote request failed reCAPTCHA for {Email}. Errors: {Errors}",
                request.Email,
                string.Join(",", verifyResult.ErrorCodes));
            return BadRequest(new { ok = false, message = "Please complete the CAPTCHA and try again." });
        }

        try
        {
            await _quoteSubmissionService.SubmitAsync(request, cancellationToken);
            return new JsonResult(new { ok = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process quote request for {Email}", request.Email);
            return StatusCode(500, new { ok = false, message = "Unable to submit right now. Please email directly." });
        }
    }
}
