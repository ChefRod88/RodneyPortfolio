using System.Text.Json;
using ChurchWebsite.Models;
using Microsoft.AspNetCore.Http;

namespace ChurchWebsite.Services;

/// <summary>Server-side IP location lookup for the Location page.</summary>
public class LocationService : ILocationService
{
    private const string TestIpForLocalhost = "8.8.8.8";
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly HttpClient _httpClient;
    private readonly ILogger<LocationService> _logger;

    public LocationService(
        IHttpContextAccessor httpContextAccessor,
        HttpClient httpClient,
        ILogger<LocationService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Attempts to resolve the current user location from IP.
    /// Returns a failure payload instead of throwing.
    /// </summary>
    public async Task<UserLocation> GetCurrentLocationAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var ip = ResolveClientIp() ?? TestIpForLocalhost;

            // ip-api.com free endpoint for reverse IP lookups.
            var endpoint = $"http://ip-api.com/json/{ip}?fields=status,message,country,countryCode,regionName,city,lat,lon";
            using var response = await _httpClient.GetAsync(endpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return Failure($"ip-api http {(int)response.StatusCode}");
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var api = await JsonSerializer.DeserializeAsync<IpApiResponse>(stream, cancellationToken: cancellationToken);
            if (api is null || !string.Equals(api.Status, "success", StringComparison.OrdinalIgnoreCase))
            {
                return Failure(api?.Message ?? "ip-api lookup failed");
            }

            return new UserLocation
            {
                City = api.City ?? string.Empty,
                Region = api.RegionName ?? string.Empty,
                Country = api.Country ?? string.Empty,
                CountryCode = api.CountryCode ?? string.Empty,
                Latitude = api.Lat,
                Longitude = api.Lon,
                Source = "server-ip",
                IsSuccess = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Server IP geolocation failed");
            return Failure("exception");
        }
    }

    private string? ResolveClientIp()
    {
        var ctx = _httpContextAccessor.HttpContext;
        if (ctx is null)
        {
            return null;
        }

        // Respect proxy header first when present.
        var forwarded = ctx.Request.Headers["X-Forwarded-For"].ToString();
        var firstForwardedIp = forwarded.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault();
        var candidate = string.IsNullOrWhiteSpace(firstForwardedIp)
            ? ctx.Connection.RemoteIpAddress?.ToString()
            : firstForwardedIp;

        // Localhost addresses are not geo-resolvable; use a deterministic test IP for dev UX.
        if (string.IsNullOrWhiteSpace(candidate) ||
            candidate == "::1" ||
            candidate == "127.0.0.1" ||
            candidate.StartsWith("::ffff:127.0.0.1", StringComparison.OrdinalIgnoreCase))
        {
            return TestIpForLocalhost;
        }

        return candidate;
    }

    private static UserLocation Failure(string reason) => new()
    {
        IsSuccess = false,
        Source = "server-ip",
        Error = reason
    };

    private sealed class IpApiResponse
    {
        public string? Status { get; set; }
        public string? Message { get; set; }
        public string? Country { get; set; }
        public string? CountryCode { get; set; }
        public string? RegionName { get; set; }
        public string? City { get; set; }
        public double? Lat { get; set; }
        public double? Lon { get; set; }
    }
}
