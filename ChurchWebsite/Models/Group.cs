namespace ChurchWebsite.Models;

/// <summary>Group model for Groups list/detail. USE CASE: Small groups, ministries.</summary>
public class Group
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Category { get; set; }        // e.g. "Men", "Women", "Young Adults"
    public string? MeetingSchedule { get; set; }  // e.g. "Tuesday 6:30 PM"
    public string? Location { get; set; }
    public string? ContactEmail { get; set; }
}
