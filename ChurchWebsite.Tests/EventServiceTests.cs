using ChurchWebsite.Services;

namespace ChurchWebsite.Tests;

public class EventServiceTests
{
    private readonly EventService _service = new();

    [Fact]
    public void GetAll_ReturnsEvents()
    {
        var result = _service.GetAll();
        Assert.NotEmpty(result);
    }

    [Fact]
    public void GetAll_ReturnsEventsSoonestFirst()
    {
        var result = _service.GetAll();
        for (int i = 0; i < result.Count - 1; i++)
            Assert.True(result[i].Date <= result[i + 1].Date);
    }

    [Fact]
    public void GetById_ReturnsCorrectEvent()
    {
        var evt = _service.GetById(1);
        Assert.NotNull(evt);
        Assert.Equal(1, evt.Id);
    }

    [Fact]
    public void GetById_ReturnsNullForMissingId()
    {
        var evt = _service.GetById(9999);
        Assert.Null(evt);
    }

    [Fact]
    public void GetAll_EachEventHasTitleAndLocation()
    {
        var result = _service.GetAll();
        foreach (var e in result)
        {
            Assert.False(string.IsNullOrWhiteSpace(e.Title), $"Event {e.Id} has no title");
            Assert.False(string.IsNullOrWhiteSpace(e.Location), $"Event {e.Id} has no location");
        }
    }

    [Fact]
    public void GetAll_EachEventEndDateAfterStartDate()
    {
        var result = _service.GetAll();
        foreach (var e in result)
            Assert.True(e.EndDate >= e.Date, $"Event {e.Id} EndDate is before Date");
    }

    [Fact]
    public void GetAll_NoDuplicateIds()
    {
        var result = _service.GetAll();
        var ids = result.Select(e => e.Id).ToList();
        Assert.Equal(ids.Count, ids.Distinct().Count());
    }

    [Fact]
    public void GetAll_CapacityIsPositive()
    {
        var result = _service.GetAll();
        foreach (var e in result)
            Assert.True(e.Capacity > 0, $"Event {e.Id} has invalid capacity");
    }
}
