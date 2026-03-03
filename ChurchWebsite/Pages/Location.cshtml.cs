using ChurchWebsite.Models;
using ChurchWebsite.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace ChurchWebsite.Pages;

/// <summary>Location page. USE CASE: Show smart current-location detector UI.</summary>
public class LocationModel : PageModel
{
    private readonly ILocationService _locationService;
    private readonly ChurchSettings _churchSettings;

    public LocationModel(ILocationService locationService, IOptions<ChurchSettings> churchOptions)
    {
        _locationService = locationService;
        _churchSettings = churchOptions.Value;
    }

    public UserLocation CurrentLocation { get; private set; } = new()
    {
        IsSuccess = false,
        Source = "server-ip",
        Error = "not-loaded"
    };

    public ChurchSettings Church => _churchSettings;
    public ChurchDestinationSettings Destination => _churchSettings.Routing.ChurchDestination;
    public string RoutingProvider => _churchSettings.Routing.Provider;
    public string GraphHopperApiKey => _churchSettings.Routing.GraphHopperApiKey;

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        CurrentLocation = await _locationService.GetCurrentLocationAsync(cancellationToken);
    }
}
