using ChurchWebsite.Models;

namespace ChurchWebsite.Services;

/// <summary>In-memory group data. USE CASE: Add/edit groups; replace with DB later.</summary>
public class GroupService
{
    private static readonly List<Group> Groups =
    [
        new Group
        {
            Id = 1,
            Name = "Men's Bible Study",
            Description = "A weekly gathering for men to study Scripture, pray together, and grow in faith.",
            Category = "Men",
            MeetingSchedule = "Tuesday 6:30 PM",
            Location = "Room 101",
            ContactEmail = "mens@church.org"
        },
        new Group
        {
            Id = 2,
            Name = "Women's Fellowship",
            Description = "Women of all ages connect for study, encouragement, and fellowship.",
            Category = "Women",
            MeetingSchedule = "Wednesday 10:00 AM",
            Location = "Fellowship Hall",
            ContactEmail = "womens@church.org"
        },
        new Group
        {
            Id = 3,
            Name = "Young Adults",
            Description = "Young adults (18-35) building community, exploring faith, and serving together.",
            Category = "Young Adults",
            MeetingSchedule = "Sundays 6:00 PM",
            Location = "Youth Center",
            ContactEmail = "ya@church.org"
        },
        new Group
        {
            Id = 4,
            Name = "Sunday School",
            Description = "Age-appropriate Bible teaching for children and teens during the Sunday service.",
            Category = "Children & Youth",
            MeetingSchedule = "Sundays 9:00 AM & 11:00 AM",
            Location = "Education Building",
            ContactEmail = "sundayschool@church.org"
        },
        new Group
        {
            Id = 5,
            Name = "Worship Team",
            Description = "Music and worship ministry. Musicians and vocalists welcome.",
            Category = "Ministry",
            MeetingSchedule = "Thursday 7:00 PM (rehearsal)",
            Location = "Main Sanctuary",
            ContactEmail = "worship@church.org"
        },
        new Group
        {
            Id = 6,
            Name = "Serve Team",
            Description = "Opportunities to serve: greeting, hospitality, setup, and more.",
            Category = "Ministry",
            MeetingSchedule = "Various",
            Location = "Church campus",
            ContactEmail = "serve@church.org"
        }
    ];

    /// <summary>Returns all groups, alphabetically by name. Used by Groups/Index.</summary>
    public List<Group> GetAll() => Groups.OrderBy(g => g.Name).ToList();

    /// <summary>Returns single group by id, or null if not found. Used by Groups/Details.</summary>
    public Group? GetById(int id) => Groups.FirstOrDefault(g => g.Id == id);
}
