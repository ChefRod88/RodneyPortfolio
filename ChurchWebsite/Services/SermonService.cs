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
            Date = new DateTime(2026, 2, 8),
            Description = "Most great things that arrive in our lives come after a great delay",
            Verse = "1 Timothy 3:14-16",
            Series = "Evangelism Series",
            VideoEmbedUrl = "https://www.youtube.com/embed/3UalYiXhJjk",
        },
        new Sermon
        {
            Id = 2,
            Title = "Suffering For Right And Wrong",
            Speaker = "Rev. Dr. Frank O'Harroll SR., Senior Pastor",
            Date = new DateTime(2026, 2, 1),
            Description = "A message on suffering for righteousness and how to respond with gentleness and respect.",
            Verse = "1 Peter 3:14-15",
            Series = "Evangelism Series",
            VideoEmbedUrl = "https://www.youtube.com/embed/V6ltUVieTkQ",
        },
        new Sermon
        {
            Id = 3,
            Title = "Holding To The Traditions Passed Down",
            Speaker = "Rev. Dr. Frank O'Harroll SR., Senior Pastor",
            Date = new DateTime(2026, 1, 25),
            Description = "Holding to the traditions passed down from the apostles.",
            Series = "Evangelism Series",
            Verse = "1 Corinthians 11:2",
            VideoEmbedUrl = "https://www.youtube.com/embed/2JPQiP2VQ8o",
        },
        new Sermon
        {
            Id = 4,
            Title = "Know That Your Labor In The Lord Is Not In Vain",
            Speaker = "Rev. Dr. Frank O'Harroll SR., Senior Pastor",
            Date = new DateTime(2026, 1, 18),
            Description = "A message on the importance of laboring in the Lord and the reward of laboring in the Lord.",
            Series = "Evangelism Series",
            Verse = "1 Corinthians 15:58",
            VideoEmbedUrl = "https://www.youtube.com/embed/KR5DMwwNBvw",
        },
        new Sermon
        {
            Id = 5,
            Title = "Doctrine And Fellowship",
            Speaker = "Rev. Dr. Frank O'Harroll SR., Senior Pastor",
            Date = new DateTime(2026, 1, 11),
            Description = "Evangelism is a core value in our church. Members regularly share their faith. New believers are being discipled. Our church is known for reaching the lost. We intentionally train people to evangelize.",
            Series = "Evangelism Series",
            Verse = "Acts 2:42",
            VideoEmbedUrl = "https://www.youtube.com/embed/k0azEKzEA2Y",
        },
        new Sermon
        {
            Id = 6,
            Title = "The Mission Of The Church: Understanding The Great Commission.",
            Speaker = "Rev. Dr. Frank O'Harroll SR., Senior Pastor",
            Date = new DateTime(2026, 1, 4),
            Description = "Evangelism is a command, not a suggestion.",
            Series = "Evangelism Series",
            Verse = "Matthew 28:19-20",
            VideoEmbedUrl = "https://www.youtube.com/embed/fHyrieW-wtM",
        },

    ];

    /// <summary>Returns all sermons, newest first. Used by Index (top 3) and Sermons/Index (all).</summary>
    public List<Sermon> GetAll() => Sermons.OrderByDescending(s => s.Date).ToList();

    /// <summary>Returns single sermon by id, or null if not found. Used by Sermons/Details.</summary>
    public Sermon? GetById(int id) => Sermons.FirstOrDefault(s => s.Id == id);
}
