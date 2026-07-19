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
    }
}