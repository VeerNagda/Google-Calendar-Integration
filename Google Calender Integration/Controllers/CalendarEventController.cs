using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Mvc;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ASP.NETCoreWebApplication1.Controllers;

[ApiController]
[Route("api/[controller]")]

public class CalendarEventController : Controller
{
    private string? _accessToken;
    private string GetPrimaryCalendarId()
    {
        var tokenFile = "D:/ASP/git/ASP.NETCoreWebApplication1/ASP.NETCoreWebApplication1/Controllers/Files/tokens.json";
        var tokenJson = System.IO.File.ReadAllText(tokenFile);
        var token = JsonConvert.DeserializeObject<JObject>(tokenJson);
        _accessToken = token?.GetValue("access_token")?.ToString();
        var credential = GoogleCredential.FromAccessToken(this._accessToken);
        var service = new CalendarService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "Softcon",
        });
        var calendarListEntry = service.CalendarList.Get("primary").Execute();
        return calendarListEntry.Id;

    }
    [HttpGet("createevent")]
    public void CreateEvent()
    {
        var calendarId = GetPrimaryCalendarId();
        var credential = GoogleCredential.FromAccessToken(this._accessToken);
        var service = new CalendarService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "My Calendar App",
        });
        
        var myEvent = new Event()
        {
            Summary = "My Event",
            Location = "123 Main St",
            Description = "This is a test event",
            Creator = new Event.CreatorData { DisplayName = "Softcon" },
            Start = new EventDateTime()
            {
                DateTime = new DateTime(2023, 5, 1, 10, 0, 0),
                TimeZone = "America/Los_Angeles"
            },
            End = new EventDateTime()
            {
                DateTime = new DateTime(2023, 5, 1, 12, 0, 0),
                TimeZone = "America/Los_Angeles"
            },
            Reminders = new Event.RemindersData()
            {
                UseDefault = true
            }
        };
        
        var request = service.Events.Insert(myEvent, calendarId);
        var createdEvent = request.Execute();
        Console.WriteLine(createdEvent.Status);
    }
    
}