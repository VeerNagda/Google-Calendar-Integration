using Google;
using Microsoft.AspNetCore.Mvc;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Google_Calender_Integration.Controllers;

[ApiController]
[Route("api/[controller]")]

public class CalendarEventController : Controller
{
    private string? _accessToken;
    
    //Function to set value of _accessToken
    [HttpGet("hasaccesstoken")]
    public bool HasAccessToken()
    {
        var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var tokenFile = appDirectory + "Controllers/Files/tokens.json";
        if (System.IO.File.Exists(tokenFile))
        {
            var tokenJson = System.IO.File.ReadAllText(tokenFile);
            var token = JsonConvert.DeserializeObject<JObject>(tokenJson);
            if (!string.IsNullOrWhiteSpace(token?.GetValue("access_token")?.ToString()))
            {
                return true;
            }
        }
        return false;
    }
    
    private void AccessTokenSetter()
    {
        var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var tokenFile = appDirectory + "Controllers/Files/tokens.json";
        var tokenJson = System.IO.File.ReadAllText(tokenFile);
        var token = JsonConvert.DeserializeObject<JObject>(tokenJson);
        if (!string.IsNullOrWhiteSpace(token?.GetValue("access_token")?.ToString()))
        {
            _accessToken = token.GetValue("access_token")?.ToString();
        }
    }
    
    private string? GetPrimaryCalendarId()
    {
        while (true)
        {
            //setting the value of the access token
            AccessTokenSetter();
            var credential = GoogleCredential.FromAccessToken(this._accessToken);
            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential, 
                ApplicationName = "Softcon",
            });
            var calendarListEntry = service.CalendarList.Get("primary");
            try
            {
                var calendarList = calendarListEntry.Execute();
                return calendarList.Id;
            }
            catch (GoogleApiException ex)
            {
                if (ex.HttpStatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    new OAuthController().RefreshToken();
                }
            }
        }
    }

    // ReSharper disable once StringLiteralTypo
    [HttpPost("eventcreate")]
    public IActionResult EventCreate([FromBody] CalendarEvent eventData)
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
            Summary = eventData.Title,
            Location = eventData.Location,
            Description = eventData.Description,
            Start = new EventDateTime()
            {
                DateTime = eventData.Start,
                TimeZone = eventData.TimeZone
            },
            End = new EventDateTime()
            {
                DateTime = eventData.End,
                TimeZone = eventData.TimeZone
            },
            Reminders = new Event.RemindersData()
            {
                UseDefault = true
            }
        };
        try
        {
            var request = service.Events.Insert(myEvent, calendarId);
            var createdEvent = request.Execute();
            return Json(new { success = true, message = "Event created successfully" });
        }
        catch (GoogleApiException ex)
        {
            if (ex.HttpStatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                new OAuthController().RefreshToken();
                var request = service.Events.Insert(myEvent, calendarId);
                request.Execute();
                return Json(new { success = true, message = "Event created successfully after refreshing token" });
            }
            else
            {
                return Json(new { success = false, message = "Error creating event" });
            }
        }
    }



}

