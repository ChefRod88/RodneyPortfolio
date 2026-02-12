using ChurchWebsite.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace ChurchWebsite.Pages;

public class LiveModel : PageModel
{
    private readonly ChurchSettings _church;

    public LiveModel(IOptions<ChurchSettings> churchOptions)
    {
        _church = churchOptions.Value;
    }

    public ChurchSettings Church => _church;
}
