using ChurchWebsite.Models;
using ChurchWebsite.Services;

namespace ChurchWebsite.Tests;

/// <summary>
/// Smoke tests — verify critical paths don't throw and return usable data.
/// These act as a safety net before deployment.
/// </summary>
public class SmokeTests
{
    // ── Services return data ────────────────────────────────────────────

    [Fact]
    public void SermonService_Smoke_ReturnsAtLeastOneSermon()
    {
        var service = new SermonService();
        Assert.True(service.GetAll().Count > 0);
    }

    [Fact]
    public void EventService_Smoke_ReturnsAtLeastOneEvent()
    {
        var service = new EventService();
        Assert.True(service.GetAll().Count > 0);
    }

    [Fact]
    public void GroupService_Smoke_ReturnsAtLeastOneGroup()
    {
        var service = new GroupService();
        Assert.True(service.GetAll().Count > 0);
    }

    // ── GetById with invalid ID never throws ────────────────────────────

    [Fact]
    public void SermonService_GetById_NegativeId_DoesNotThrow()
    {
        var service = new SermonService();
        var result = service.GetById(-1);
        Assert.Null(result);
    }

    [Fact]
    public void EventService_GetById_NegativeId_DoesNotThrow()
    {
        var service = new EventService();
        var result = service.GetById(-1);
        Assert.Null(result);
    }

    [Fact]
    public void GroupService_GetById_NegativeId_DoesNotThrow()
    {
        var service = new GroupService();
        var result = service.GetById(-1);
        Assert.Null(result);
    }

    // ── Model validation ────────────────────────────────────────────────

    [Fact]
    public void ChurchSettings_GoogleMapsUrl_NeverNull()
    {
        var settings = new ChurchSettings
        {
            Address = new AddressSettings
            {
                Street = "123 Ave Y NE",
                City = "Winter Haven",
                State = "FL",
                Zip = "33881"
            }
        };
        Assert.NotNull(settings.GoogleMapsUrl);
        Assert.StartsWith("https://", settings.GoogleMapsUrl);
    }

    [Fact]
    public void ChurchSettings_GoogleMapsUrl_WithEmptyAddress_DoesNotThrow()
    {
        var settings = new ChurchSettings { Address = new AddressSettings() };
        var url = settings.GoogleMapsUrl;
        Assert.NotNull(url);
    }

    // ── Image URLs are local paths (not broken external links) ──────────

    [Fact]
    public void EventService_ImageUrls_AreLocalPaths()
    {
        var service = new EventService();
        foreach (var evt in service.GetAll())
        {
            if (!string.IsNullOrWhiteSpace(evt.ImageUrl))
                Assert.StartsWith("/images/", evt.ImageUrl);
        }
    }

    [Fact]
    public void GroupService_ImageUrls_AreLocalPaths()
    {
        var service = new GroupService();
        foreach (var group in service.GetAll())
        {
            if (!string.IsNullOrWhiteSpace(group.ImageUrl))
                Assert.StartsWith("/images/", group.ImageUrl);
        }
    }

    // ── Sermon embed URLs are YouTube ───────────────────────────────────

    [Fact]
    public void SermonService_EmbedUrls_AreYouTube()
    {
        var service = new SermonService();
        foreach (var sermon in service.GetAll())
        {
            if (!string.IsNullOrWhiteSpace(sermon.VideoEmbedUrl))
                Assert.Contains("youtube.com/embed/", sermon.VideoEmbedUrl);
        }
    }
}
