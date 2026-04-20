using System.Text.Json.Serialization;

namespace ChurchWebsite.Services;

internal sealed class PexelsSearchResponse
{
    [JsonPropertyName("photos")]
    public List<PexelsPhotoDto> Photos { get; set; } = [];
}

internal sealed class PexelsPhotoDto
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("photographer")]
    public string Photographer { get; set; } = string.Empty;

    [JsonPropertyName("photographer_url")]
    public string PhotographerUrl { get; set; } = string.Empty;

    [JsonPropertyName("alt")]
    public string? Alt { get; set; }

    [JsonPropertyName("src")]
    public PexelsSrcDto Src { get; set; } = new();
}

internal sealed class PexelsSrcDto
{
    [JsonPropertyName("large2x")]
    public string? Large2x { get; set; }

    [JsonPropertyName("large")]
    public string? Large { get; set; }
}
