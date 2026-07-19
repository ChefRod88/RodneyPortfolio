using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RodneyPortfolio.Models;
using RodneyPortfolio.Services;

namespace RodneyPortfolio.Pages;

/// <summary>
/// Model for the Agreement page, processing contract signature submissions and PDF generation forwarding.
/// </summary>
public class AgreementModel : PageModel
{
    private readonly IAgreementEmailService _emailService;
    private readonly ILogger<AgreementModel> _logger;

    public AgreementModel(IAgreementEmailService emailService, ILogger<AgreementModel> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public string ClientIp { get; set; } = string.Empty;

    public void OnGet()
    {
        ClientIp = GetClientIp();
        ViewData["Seo"] = new RodneyPortfolio.Models.SeoMetadata
        {
            Title = "Services Agreement | Rodney Chery",
            Description = "Services agreement.",
            Robots = "noindex, nofollow"
        };
    }

    private string GetClientIp()
    {
        var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
    }

    [BindProperty]
    public AgreementSubmissionInput Input { get; set; } = new();

    public async Task<IActionResult> OnPostSignAsync()
    {
        if (string.IsNullOrWhiteSpace(Input.ClientName) ||
            string.IsNullOrWhiteSpace(Input.ClientEmail) ||
            string.IsNullOrWhiteSpace(Input.ClientCompany) ||
            string.IsNullOrWhiteSpace(Input.PdfBase64))
        {
            return new JsonResult(new { success = false, message = "All fields, including signature, are required." });
        }

        // Clean/Trim the client email
        Input.ClientEmail = Input.ClientEmail.Trim().Trim(';', ',');

        // Email syntax validation
        if (!Input.ClientEmail.Contains("@") || 
            !Input.ClientEmail.Contains(".") || 
            !System.Net.Mail.MailAddress.TryCreate(Input.ClientEmail, out _))
        {
            return new JsonResult(new { success = false, message = "Please enter a valid email address." });
        }

        try
        {
            // Remove Data URI header if present
            var base64Data = Input.PdfBase64;
            if (base64Data.Contains(","))
            {
                base64Data = base64Data.Split(',')[1];
            }
            Input.PdfBase64 = base64Data;

            await _emailService.SendAsync(Input);
            return new JsonResult(new { success = true, message = "Agreement signed successfully! A copy has been emailed to you." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing contract signature for {Company}", Input.ClientCompany);
            return new JsonResult(new { success = false, message = "An error occurred while sending the email. Please try again." });
        }
    }
}
