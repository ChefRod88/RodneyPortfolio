namespace ChurchWebsite.Models;

public class Group
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? MeetingSchedule { get; set; }
    public string? Location { get; set; }
    public string? ContactEmail { get; set; }
}
