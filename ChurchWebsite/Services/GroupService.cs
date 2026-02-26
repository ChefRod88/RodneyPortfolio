using ChurchWebsite.Models;

namespace ChurchWebsite.Services;

/// <summary>In-memory group data. USE CASE: Add/edit groups; replace with DB later.</summary>
public class GroupService : IGroupService
{
    private static readonly List<Group> Groups =
    [
        new Group
        {
            Id = 1,
            Name = "Men's Ministry",
            Description = "Men's Ministry is a place where men can come together to grow in their faith and serve the Lord.",
            Category = "Men",
            MeetingSchedule = "Every 2nd Saturday of the month at 9 AM for men's breakfast, prayer, worship and fellowship",
            Location = "New Bethel Missionary Baptist Church",
            ContactEmail = "placeholder@church.org",
            ImageUrl = "/images/NBMBC_MEN.png"
        },
        new Group
        {
            Id = 2,
            Name = "Kids' Ministry",
            Description = "Kids' Ministry is a place where kids can come together to learn about Jesus and grow in their faith.",
            Category = "Kids",
            MeetingSchedule = "TBA",
            Location = "New Bethel Missionary Baptist Church",
            ContactEmail = "placeholder@church.org",
            ImageUrl = "/images/IMG_7732.jpg"
        },
        new Group
        {
            Id = 3,
            Name = "City Wide Mission",
            Description = "City Wide Mission is a place where we can come together to serve the Lord and the community.",
            Category = "City Wide Mission",
            MeetingSchedule = "TBA",
            Location = "TBA",
            ContactEmail = "placeholder@church.org",
            ImageUrl = "/images/CityWideMission.png"
        }
    ];

    /// <summary>Returns all groups, alphabetically by name. Used by Groups/Index.</summary>
    public List<Group> GetAll() => Groups.OrderBy(g => g.Name).ToList();

    /// <summary>Returns single group by id, or null if not found. Used by Groups/Details.</summary>
    public Group? GetById(int id) => Groups.FirstOrDefault(g => g.Id == id);
}
