namespace ChurchWebsite.Models;

/// <summary>Church configuration from appsettings.json. USE CASE: Edit church name, hero image, contact info.</summary>
public class ChurchSettings
{
    public const string SectionName = "Church";  // Config key in appsettings.json

    public string Name { get; set; } = string.Empty;           // Church name - nav, footer, page titles
    public string Tagline { get; set; } = string.Empty;         // Short tagline; fallback for hero headline
    public string HeroImageUrl { get; set; } = string.Empty;     // Path to hero bg image (e.g. /images/hero.jpg)
    public string HeroHeadline { get; set; } = string.Empty;    // Main headline on home hero
    public string MissionStatement { get; set; } = string.Empty;  // Blue mission section on home
    public string MissionSubtext { get; set; } = string.Empty;  // Subtext under mission
    public List<string> ServiceTimes { get; set; } = [];       // e.g. "Sundays at 11:00 AM"
    public AddressSettings Address { get; set; } = new();      // Street, City, State, Zip
    public string Phone { get; set; } = string.Empty;          // Footer, contact
    public string Email { get; set; } = string.Empty;         // Footer, contact
    public string LiveStreamUrl { get; set; } = string.Empty;  // YouTube embed URL for Live page
    public string LiveStreamPlaceholderImageUrl { get; set; } = string.Empty;  // Thumbnail when stream unavailable; fallback to HeroImageUrl
    public SocialMediaSettings SocialMedia { get; set; } = new();  // Facebook, YouTube, Instagram

    /// <summary>Formatted address: "Street, City, State Zip"</summary>
    public string FullAddress => string.Join(", ",
        new[] { Address.Street, Address.City, $"{Address.State} {Address.Zip}" }
            .Where(s => !string.IsNullOrWhiteSpace(s)));

    /// <summary>Google Maps search URL for church address; used in nav Location link</summary>
    public string GoogleMapsUrl => !string.IsNullOrWhiteSpace(FullAddress)
        ? $"https://www.google.com/maps/search/?api=1&query={Uri.EscapeDataString(FullAddress)}"
        : string.Empty;
}

/// <summary>Address for footer and maps. USE CASE: Church location.</summary>
public class AddressSettings
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Zip { get; set; } = string.Empty;
}

/// <summary>Social links for footer. USE CASE: Facebook, YouTube, Instagram URLs.</summary>
public class SocialMediaSettings
{
    public string Facebook { get; set; } = string.Empty;
    public string YouTube { get; set; } = string.Empty;
    public string Instagram { get; set; } = string.Empty;
}
