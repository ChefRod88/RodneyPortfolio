using ChurchWebsite.Models;

namespace ChurchWebsite.Services;

/// <summary>In-memory event data. USE CASE: Add/edit events; replace with DB later.</summary>
public class EventService : IEventService
{
    private static readonly List<ChurchEvent> Events =
    [
        new ChurchEvent
        {
            Id = 1,
            Title = "Sunday Worship Service",
            Description = "Join us for worship, teaching, and fellowship. Every Sunday at 11 AM. First Sunday of each month at 8 AM. All are welcome.",
            Date = new DateTime(2026, 2, 15, 11, 0, 0),
            EndDate = new DateTime(2026, 2, 15, 12, 30, 0),
            Location = "New Bethel Missionary Baptist Church",
            Capacity = 300,
            ImageUrl = "/images/IMG_7706.jpg"
        },
        new ChurchEvent
        {
            Id = 2,
            Title = "Event Placeholder 1",
            Description = "Event details coming soon.",
            Date = new DateTime(2026, 2, 20, 18, 0, 0),
            EndDate = new DateTime(2026, 2, 20, 20, 0, 0),
            Location = "TBA",
            Capacity = 50,
            ImageUrl = "/images/IMG_7631.jpg"
        },
        new ChurchEvent
        {
            Id = 3,
            Title = "Event Placeholder 2",
            Description = "Event details coming soon.",
            Date = new DateTime(2026, 2, 25, 9, 0, 0),
            EndDate = new DateTime(2026, 2, 25, 12, 0, 0),
            Location = "TBA",
            Capacity = 25,
            ImageUrl = "/images/IMG_7728.jpg"
        },
        new ChurchEvent
        {
            Id = 4,
            Title = "Event Placeholder 3",
            Description = "Event details coming soon.",
            Date = new DateTime(2026, 3, 1, 8, 0, 0),
            EndDate = new DateTime(2026, 3, 1, 14, 0, 0),
            Location = "TBA",
            Capacity = 75,
            ImageUrl = "/images/IMG_7729.JPG"
        },
        new ChurchEvent
        {
            Id = 5,
            Title = "Event Placeholder 4",
            Description = "Event details coming soon.",
            Date = new DateTime(2026, 3, 10, 10, 0, 0),
            EndDate = new DateTime(2026, 3, 10, 12, 0, 0),
            Location = "TBA",
            Capacity = 100,
            ImageUrl = "/images/IMG_7732.jpg"
        }
    ];

    /// <summary>Returns all events, soonest first. Used by Events/Index.</summary>
    public List<ChurchEvent> GetAll() => Events
        .OrderBy(e => e.Date)
        .ToList();

    /// <summary>Returns single event by id, or null if not found. Used by Events/Details.</summary>
    public ChurchEvent? GetById(int id) => Events.FirstOrDefault(e => e.Id == id);
}
