using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RodneyPortfolio.Models;
using RodneyPortfolio.Services;

namespace RodneyPortfolio.Pages;

public class IndexModel : PageModel
{
    private readonly IQuoteSubmissionService _quoteSubmissionService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(IQuoteSubmissionService quoteSubmissionService, ILogger<IndexModel> logger)
    {
        _quoteSubmissionService = quoteSubmissionService;
        _logger = logger;
    }

    public void OnGet()
    {

    }

    public async Task<IActionResult> OnPostQuoteAsync([FromForm] QuoteRequestInput request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { ok = false, message = "Please complete all required fields." });
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
