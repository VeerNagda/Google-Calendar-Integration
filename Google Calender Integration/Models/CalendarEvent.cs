namespace Google_Calender_Integration.Models;

public class CalendarEvent
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public DateTime Start { get; set; }

    public DateTime End { get; set; }

    public string? Location { get; set; }

    public string? TimeZone { get; set; }
}