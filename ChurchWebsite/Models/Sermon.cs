namespace ChurchWebsite.Models;

/// <summary>Sermon model for Sermons list/detail. USE CASE: Video/audio sermon library.</summary>
public class Sermon
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Speaker { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Series { get; set; }           // e.g. "Faith Journey"
    public string? VideoEmbedUrl { get; set; }   // YouTube embed URL for Details page
    public string? AudioUrl { get; set; }
    public string? DownloadUrl { get; set; }
}
