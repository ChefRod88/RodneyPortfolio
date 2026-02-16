using ChurchWebsite.Models;

namespace ChurchWebsite.Services;

/// <summary>In-memory sermon data. USE CASE: Add/edit sermons; replace with DB later.</summary>
public class SermonService
{
    private static readonly List<Sermon> Sermons =
    [
        new Sermon
        {
            Id = 1,
            Title = "You Live Life Better When You Understand Life Is Full Of Delays.",
            Speaker = "Rev. Dr. Frank O'Harroll SR., Senior Pastor",
            Date = new DateTime(2025, 2, 8),
            Description = "Most great things that arrive in our lives come after a great delay",
            Series = "Evangelism Series",
            VideoEmbedUrl = "https://www.youtube.com/embed/3UalYiXhJjk",
            
            
        }
    ];

    /// <summary>Returns all sermons, newest first. Used by Index (top 3) and Sermons/Index (all).</summary>
    public List<Sermon> GetAll() => Sermons.OrderByDescending(s => s.Date).ToList();

    /// <summary>Returns single sermon by id, or null if not found. Used by Sermons/Details.</summary>
    public Sermon? GetById(int id) => Sermons.FirstOrDefault(s => s.Id == id);
}
