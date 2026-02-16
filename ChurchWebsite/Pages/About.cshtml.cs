using ChurchWebsite.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace ChurchWebsite.Pages;

/// <summary>About page. USE CASE: Church info, beliefs, leadership.</summary>
public class AboutModel : PageModel
{
    private readonly ChurchSettings _church;

    public AboutModel(IOptions<ChurchSettings> churchOptions)
    {
        _church = churchOptions.Value;
    }

    public ChurchSettings Church => _church;
}
