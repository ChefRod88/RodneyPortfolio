namespace ChurchWebsite.Models;

public class ChurchSettings
{
    public const string SectionName = "Church";

    public string Name { get; set; } = string.Empty;
    public string Tagline { get; set; } = string.Empty;
    public string HeroImageUrl { get; set; } = string.Empty;
    public string HeroHeadline { get; set; } = string.Empty;
    public string MissionStatement { get; set; } = string.Empty;
    public string MissionSubtext { get; set; } = string.Empty;
    public List<string> ServiceTimes { get; set; } = [];
    public AddressSettings Address { get; set; } = new();
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string LiveStreamUrl { get; set; } = string.Empty;
    public SocialMediaSettings SocialMedia { get; set; } = new();

    public string FullAddress => string.Join(", ",
        new[] { Address.Street, Address.City, $"{Address.State} {Address.Zip}" }
            .Where(s => !string.IsNullOrWhiteSpace(s)));

    public string GoogleMapsUrl => !string.IsNullOrWhiteSpace(FullAddress)
        ? $"https://www.google.com/maps/search/?api=1&query={Uri.EscapeDataString(FullAddress)}"
        : string.Empty;
}

public class AddressSettings
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Zip { get; set; } = string.Empty;
}

public class SocialMediaSettings
{
    public string Facebook { get; set; } = string.Empty;
    public string YouTube { get; set; } = string.Empty;
    public string Instagram { get; set; } = string.Empty;
}
