namespace RodneyPortfolio.Models;

public class Article
{
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTime PublishedDate { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public string HtmlContent { get; set; } = string.Empty;
    public int ReadingTimeMinutes { get; set; }
}
