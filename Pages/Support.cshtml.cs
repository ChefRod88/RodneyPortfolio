using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using RodneyPortfolio.Models;
using RodneyPortfolio.Services;

namespace RodneyPortfolio.Pages;

public class SupportModel : PageModel
{
    private readonly ISupportRequestSubmissionService _supportSubmissionService;
    private readonly ITurnstileVerificationService _turnstileVerificationService;
    private readonly TurnstileOptions _turnstileOptions;
    private readonly ILogger<SupportModel> _logger;

    public string TurnstileSiteKey => _turnstileOptions.SiteKey;

    public SupportModel(
        ISupportRequestSubmissionService supportSubmissionService,
        ITurnstileVerificationService turnstileVerificationService,
        IOptions<TurnstileOptions> turnstileOptions,
        ILogger<SupportModel> logger)
    {
        _supportSubmissionService = supportSubmissionService;
        _turnstileVerificationService = turnstileVerificationService;
        _turnstileOptions = turnstileOptions.Value;
        _logger = logger;
    }

    public void OnGet()
    {
    }

    [EnableRateLimiting("SupportPolicy")]
    public async Task<IActionResult> OnPostAsync(
        [FromForm] PublicSupportRequestInput request,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.Website))
        {
            _logger.LogWarning(
                "Support request blocked by honeypot from {IpAddress}",
                HttpContext.Connection.RemoteIpAddress);
            return BadRequest(new { ok = false, message = "Unable to submit request." });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(new { ok = false, message = "Please complete all required fields." });
        }

        var verifyResult = await _turnstileVerificationService.VerifyAsync(
            request.TurnstileToken ?? string.Empty,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            cancellationToken);
        if (!verifyResult.IsSuccess)
        {
            _logger.LogWarning(
                "Support request failed Turnstile verification for {Email}. Errors: {Errors}",
                request.Email,
                string.Join(",", verifyResult.ErrorCodes));
            return BadRequest(new { ok = false, message = "Please complete the CAPTCHA and try again." });
        }

        try
        {
            await _supportSubmissionService.SubmitAsync(request, cancellationToken);
            return new JsonResult(new { ok = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process support request for {Email}", request.Email);
            return StatusCode(500, new { ok = false, message = "Unable to submit right now. Please email rodney@globalrcdev.com directly." });
        }
    }
}
