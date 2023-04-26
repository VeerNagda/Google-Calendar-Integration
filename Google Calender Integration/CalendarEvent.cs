using System.Runtime.InteropServices.JavaScript;

namespace Google_Calender_Integration;

public class CalenderEvent
{
    public string? Title { get; set; }
    
    public string? Description { get; set; }
    
    public DateTime StartDate { get; set; }
    
    public DateTime EndDate { get; set; }
    
    public string? Location { get; set; }
    
}