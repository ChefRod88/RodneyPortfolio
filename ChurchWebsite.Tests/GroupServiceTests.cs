using ChurchWebsite.Services;

namespace ChurchWebsite.Tests;

public class GroupServiceTests
{
    private readonly GroupService _service = new();

    [Fact]
    public void GetAll_ReturnsGroups()
    {
        var result = _service.GetAll();
        Assert.NotEmpty(result);
    }

    [Fact]
    public void GetAll_ReturnsGroupsAlphabetically()
    {
        var result = _service.GetAll();
        var names = result.Select(g => g.Name).ToList();
        var sorted = names.OrderBy(n => n).ToList();
        Assert.Equal(sorted, names);
    }

    [Fact]
    public void GetById_ReturnsCorrectGroup()
    {
        var group = _service.GetById(1);
        Assert.NotNull(group);
        Assert.Equal(1, group.Id);
    }

    [Fact]
    public void GetById_ReturnsNullForMissingId()
    {
        var group = _service.GetById(9999);
        Assert.Null(group);
    }

    [Fact]
    public void GetAll_EachGroupHasNameCategoryAndContact()
    {
        var result = _service.GetAll();
        foreach (var g in result)
        {
            Assert.False(string.IsNullOrWhiteSpace(g.Name), $"Group {g.Id} has no name");
            Assert.False(string.IsNullOrWhiteSpace(g.Category), $"Group {g.Id} has no category");
            Assert.False(string.IsNullOrWhiteSpace(g.ContactEmail), $"Group {g.Id} has no contact email");
        }
    }

    [Fact]
    public void GetAll_NoDuplicateIds()
    {
        var result = _service.GetAll();
        var ids = result.Select(g => g.Id).ToList();
        Assert.Equal(ids.Count, ids.Distinct().Count());
    }

    [Fact]
    public void GetAll_ContactEmailsAreValid()
    {
        var result = _service.GetAll();
        foreach (var g in result)
            Assert.Contains("@", g.ContactEmail);
    }
}
