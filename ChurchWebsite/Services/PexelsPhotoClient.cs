using System.Text.Json;
using ChurchWebsite.Models;
using Microsoft.Extensions.Options;

namespace ChurchWebsite.Services;

public sealed class PexelsPhotoClient : IPexelsPhotoClient
{
    private readonly HttpClient _http;
    private readonly IOptions<PexelsOptions> _options;
    private readonly ILogger<PexelsPhotoClient> _log;

    public PexelsPhotoClient(HttpClient http, IOptions<PexelsOptions> options, ILogger<PexelsPhotoClient> log)
    {
        _http = http;
        _options = options;
        _log = log;
    }

    public async Task<ChurchPexelsPhoto?> SearchFirstAsync(string query, CancellationToken cancellationToken = default)
    {
        var key = _options.Value.ApiKey?.Trim();
        if (string.IsNullOrEmpty(key))
            return null;

        if (string.IsNullOrWhiteSpace(query))
            return null;

        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Get,
                $"v1/search?query={Uri.EscapeDataString(query.Trim())}&per_page=1");
            // Pexels: send the API key as the entire Authorization header value (not Bearer).
            req.Headers.TryAddWithoutValidation("Authorization", key);

            using var resp = await _http.SendAsync(req, cancellationToken).ConfigureAwait(false);
            if (!resp.IsSuccessStatusCode)
            {
                _log.LogWarning("Pexels API returned {Status} for query {Query}", (int)resp.StatusCode, query);
                return null;
            }

            await using var stream = await resp.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            var parsed = await JsonSerializer.DeserializeAsync<PexelsSearchResponse>(stream,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, cancellationToken).ConfigureAwait(false);

            var dto = parsed?.Photos?.FirstOrDefault();
            if (dto is null)
                return null;

            var src = !string.IsNullOrWhiteSpace(dto.Src.Large2x) ? dto.Src.Large2x! : dto.Src.Large;
            if (string.IsNullOrWhiteSpace(src))
                return null;

            var alt = !string.IsNullOrWhiteSpace(dto.Alt)
                ? dto.Alt!
                : $"Church-themed stock photo by {dto.Photographer}";

            return new ChurchPexelsPhoto
            {
                ImageUrl = src,
                AltText = alt,
                PhotographerName = dto.Photographer,
                PhotographerUrl = string.IsNullOrWhiteSpace(dto.PhotographerUrl) ? "https://www.pexels.com" : dto.PhotographerUrl,
                PhotoPageUrl = string.IsNullOrWhiteSpace(dto.Url) ? "https://www.pexels.com" : dto.Url
            };
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Pexels search failed for {Query}", query);
            return null;
        }
    }
}
