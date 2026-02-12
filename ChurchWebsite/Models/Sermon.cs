namespace ChurchWebsite.Models;

public class Sermon
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Speaker { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Series { get; set; }
    public string? VideoEmbedUrl { get; set; }
    public string? AudioUrl { get; set; }
    public string? DownloadUrl { get; set; }
}
