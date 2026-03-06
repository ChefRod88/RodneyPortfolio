using ChurchWebsite.Services;

namespace ChurchWebsite.Tests;

public class SermonServiceTests
{
    private readonly SermonService _service = new();

    [Fact]
    public void GetAll_ReturnsSermons()
    {
        var result = _service.GetAll();
        Assert.NotEmpty(result);
    }

    [Fact]
    public void GetAll_ReturnsSermonsNewestFirst()
    {
        var result = _service.GetAll();
        for (int i = 0; i < result.Count - 1; i++)
            Assert.True(result[i].Date >= result[i + 1].Date);
    }

    [Fact]
    public void GetById_ReturnsCorrectSermon()
    {
        var sermon = _service.GetById(1);
        Assert.NotNull(sermon);
        Assert.Equal(1, sermon.Id);
    }

    [Fact]
    public void GetById_ReturnsNullForMissingId()
    {
        var sermon = _service.GetById(9999);
        Assert.Null(sermon);
    }

    [Fact]
    public void GetAll_EachSermonHasTitleAndSpeaker()
    {
        var result = _service.GetAll();
        foreach (var s in result)
        {
            Assert.False(string.IsNullOrWhiteSpace(s.Title), $"Sermon {s.Id} has no title");
            Assert.False(string.IsNullOrWhiteSpace(s.Speaker), $"Sermon {s.Id} has no speaker");
        }
    }

    [Fact]
    public void GetAll_EachSermonHasVideoEmbedUrl()
    {
        var result = _service.GetAll();
        foreach (var s in result)
            Assert.False(string.IsNullOrWhiteSpace(s.VideoEmbedUrl), $"Sermon {s.Id} has no embed URL");
    }

    [Fact]
    public void GetAll_NoDuplicateIds()
    {
        var result = _service.GetAll();
        var ids = result.Select(s => s.Id).ToList();
        Assert.Equal(ids.Count, ids.Distinct().Count());
    }
}
