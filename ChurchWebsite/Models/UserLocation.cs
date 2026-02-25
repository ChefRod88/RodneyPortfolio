namespace ChurchWebsite.Models;

/// <summary>User location payload for Location page display.</summary>
public class UserLocation
{
    public string City { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string Source { get; set; } = "unknown";
    public bool IsSuccess { get; set; }
    public string Error { get; set; } = string.Empty;

    /// <summary>Human-readable location text such as "Winter Haven, Florida, USA".</summary>
    public string DisplayText
    {
        get
        {
            var normalizedCountry =
                string.Equals(CountryCode, "US", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(Country, "United States", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(Country, "United States of America", StringComparison.OrdinalIgnoreCase)
                    ? "USA"
                    : Country;

            var parts = new[] { City, Region, normalizedCountry }
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToArray();

            return parts.Length > 0 ? string.Join(", ", parts) : "Location unavailable";
        }
    }
}
