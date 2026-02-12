using ChurchWebsite.Models;

namespace ChurchWebsite.Services;

public class EventService
{
    private static readonly List<ChurchEvent> Events =
    [
        new ChurchEvent
        {
            Id = 1,
            Title = "Sunday Worship Service",
            Description = "Join us for worship, teaching, and fellowship. All are welcome.",
            Date = new DateTime(2026, 2, 15, 9, 0, 0),
            EndDate = new DateTime(2026, 2, 15, 10, 30, 0),
            Location = "Main Sanctuary",
            Capacity = 300
        },
        new ChurchEvent
        {
            Id = 2,
            Title = "Youth Group Meeting",
            Description = "Middle and high school students gather for games, teaching, and community.",
            Date = new DateTime(2026, 2, 14, 18, 0, 0),
            EndDate = new DateTime(2026, 2, 14, 20, 0, 0),
            Location = "Youth Center",
            Capacity = 50
        },
        new ChurchEvent
        {
            Id = 3,
            Title = "Small Group Leader Training",
            Description = "Training for current and prospective small group leaders.",
            Date = new DateTime(2026, 2, 21, 9, 0, 0),
            EndDate = new DateTime(2026, 2, 21, 12, 0, 0),
            Location = "Conference Room A",
            Capacity = 25
        },
        new ChurchEvent
        {
            Id = 4,
            Title = "Community Outreach Day",
            Description = "Serve our community together. Food drive, yard work, and more.",
            Date = new DateTime(2026, 2, 28, 8, 0, 0),
            EndDate = new DateTime(2026, 2, 28, 14, 0, 0),
            Location = "Meet at Church",
            Capacity = 75
        },
        new ChurchEvent
        {
            Id = 5,
            Title = "Easter Celebration",
            Description = "Special Easter service with music, message, and family activities.",
            Date = new DateTime(2026, 4, 5, 10, 0, 0),
            EndDate = new DateTime(2026, 4, 5, 12, 0, 0),
            Location = "Main Sanctuary",
            Capacity = 500
        }
    ];

    public List<ChurchEvent> GetAll() => Events
        .OrderBy(e => e.Date)
        .ToList();

    public ChurchEvent? GetById(int id) => Events.FirstOrDefault(e => e.Id == id);
}
