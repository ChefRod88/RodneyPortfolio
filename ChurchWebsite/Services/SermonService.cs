using ChurchWebsite.Models;

namespace ChurchWebsite.Services;

public class SermonService
{
    private static readonly List<Sermon> Sermons =
    [
        new Sermon
        {
            Id = 1,
            Title = "Living by Faith",
            Speaker = "Pastor Name",
            Date = new DateTime(2025, 2, 9),
            Description = "A message about trusting God in uncertain times and stepping out in faith when the path ahead is unclear.",
            Series = "Faith Journey",
            VideoEmbedUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ",
            AudioUrl = "#",
            DownloadUrl = "#"
        },
        new Sermon
        {
            Id = 2,
            Title = "The Power of Community",
            Speaker = "Pastor Name",
            Date = new DateTime(2025, 2, 2),
            Description = "Exploring how we are called to live in community, support one another, and grow together in faith.",
            Series = "Together",
            VideoEmbedUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ",
            AudioUrl = "#",
            DownloadUrl = "#"
        },
        new Sermon
        {
            Id = 3,
            Title = "Rest and Renewal",
            Speaker = "Associate Pastor",
            Date = new DateTime(2025, 1, 26),
            Description = "Finding rest in God's presence and discovering renewal when life feels overwhelming.",
            Series = "Sabbath",
            VideoEmbedUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ",
            AudioUrl = "#",
            DownloadUrl = "#"
        },
        new Sermon
        {
            Id = 4,
            Title = "Hope for the New Year",
            Speaker = "Pastor Name",
            Date = new DateTime(2025, 1, 5),
            Description = "Starting the new year with hope and intention, anchored in God's promises.",
            Series = null,
            VideoEmbedUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ",
            AudioUrl = "#",
            DownloadUrl = "#"
        },
        new Sermon
        {
            Id = 5,
            Title = "The Gift of Salvation",
            Speaker = "Pastor Name",
            Date = new DateTime(2024, 12, 15),
            Description = "Celebrating the true meaning of Christmas and the gift of salvation through Christ.",
            Series = "Advent",
            VideoEmbedUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ",
            AudioUrl = "#",
            DownloadUrl = "#"
        }
    ];

    public List<Sermon> GetAll() => Sermons.OrderByDescending(s => s.Date).ToList();

    public Sermon? GetById(int id) => Sermons.FirstOrDefault(s => s.Id == id);
}
