using ChurchWebsite.Models;
using ChurchWebsite.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace ChurchWebsite.Pages;

/// <summary>Home page. USE CASE: Hero, cards, mission, ministries, latest sermons.</summary>
public class IndexModel : PageModel
{
    private readonly ChurchSettings _church;
    private readonly ISermonService _sermonService;
    private readonly IChurchImageryRegistry _churchPhotos;

    public IndexModel(
    IOptions<ChurchSettings> churchOptions,
    ISermonService sermonService,
    IChurchImageryRegistry churchPhotos)
{
    _church = churchOptions.Value;
    _sermonService = sermonService;
    _churchPhotos = churchPhotos;
}

    public ChurchSettings Church => _church;       // Exposed to view for hero, mission, etc.
    public List<Sermon> LatestSermons { get; set; } = [];  // Top 3 sermons for home page
    public IReadOnlyList<HeroSlideResolved> HomeHeroSlides { get; private set; } = [];

    /// <summary>Loads latest 3 sermons from SermonService for home page display</summary>
    public void OnGet()
    {
        LatestSermons = _sermonService.GetAll().Take(3).ToList();
        HomeHeroSlides = BuildHomeHeroSlides();
    }

    private IReadOnlyList<HeroSlideResolved> BuildHomeHeroSlides()
    {
        var list = new List<HeroSlideResolved>();
        var seenUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var defaultAlt = string.IsNullOrWhiteSpace(_church.NameText)
            ? "New Bethel Missionary Baptist Church"
            : _church.NameText;

        void Add(string? url, string? alt)
        {
            if (string.IsNullOrWhiteSpace(url)) return;
            var u = url.Trim();
            if (!seenUrls.Add(u)) return;
            list.Add(new HeroSlideResolved(u, string.IsNullOrWhiteSpace(alt) ? defaultAlt : alt.Trim()));
        }

        void AddFront(string? url, string? alt)
        {
            if (string.IsNullOrWhiteSpace(url)) return;
            var u = url.Trim();
            if (!seenUrls.Add(u)) return;
            list.Insert(0, new HeroSlideResolved(u, string.IsNullOrWhiteSpace(alt) ? defaultAlt : alt.Trim()));
        }

        if (_church.HeroSlidePexelsSlots.Count > 0 && !string.IsNullOrWhiteSpace(_church.HeroImageUrl))
            AddFront(_church.HeroImageUrl, defaultAlt);

        foreach (var s in _church.HeroSlides)
            Add(s.ImageUrl, s.Alt);

        foreach (var slot in _church.HeroSlidePexelsSlots)
        {
            if (string.IsNullOrWhiteSpace(slot)) continue;
            var ph = _churchPhotos.Get(slot.Trim());
            if (ph is null) continue;
            Add(ph.ImageUrl, ph.AltText);
        }

        if (list.Count == 0)
            Add(_church.HeroImageUrl, defaultAlt);

        if (list.Count == 0)
        {
            var fallback = _churchPhotos.Get("HomeHero");
            if (fallback is not null)
                Add(fallback.ImageUrl, fallback.AltText);
        }

        return list;
    }
}

/// <summary>Resolved hero slide for the home rotor (config + Pexels).</summary>
public sealed record HeroSlideResolved(string Url, string Alt);
