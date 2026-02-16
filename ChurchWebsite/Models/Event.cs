namespace ChurchWebsite.Models;

/// <summary>Event model for Events list/detail. USE CASE: Church events, service times.</summary>
public class ChurchEvent
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Location { get; set; }
    public int? Capacity { get; set; }
    public string? ImageUrl { get; set; }
}
