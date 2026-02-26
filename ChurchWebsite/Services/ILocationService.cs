using ChurchWebsite.Models;

namespace ChurchWebsite.Services;

public interface ILocationService
{
    Task<UserLocation> GetCurrentLocationAsync(CancellationToken cancellationToken = default);
}
