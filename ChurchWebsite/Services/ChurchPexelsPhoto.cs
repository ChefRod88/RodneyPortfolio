namespace ChurchWebsite.Services;

/// <summary>Resolved Pexels image for a page slot (attribution required when displaying).</summary>
public sealed class ChurchPexelsPhoto
{
    public required string ImageUrl { get; init; }
    public required string AltText { get; init; }
    public required string PhotographerName { get; init; }
    public required string PhotographerUrl { get; init; }
    public required string PhotoPageUrl { get; init; }
}
