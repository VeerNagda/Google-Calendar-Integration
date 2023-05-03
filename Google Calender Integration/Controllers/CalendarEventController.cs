using System.Net;
using Google;
using Google_Calender_Integration.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Google_Calender_Integration.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CalendarEventController : Controller
{
    private static string? _accessToken;
    private static string? _calendarId;

    //Function to set value of _accessToken
    // ReSharper disable once StringLiteralTypo
    [HttpGet("hasaccesstoken")]
    public bool HasAccessToken()
    {
        var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var tokenFile = appDirectory + "Controllers/Files/tokens.json";
        if (!System.IO.File.Exists(tokenFile)) return false;
        var tokenJson = System.IO.File.ReadAllText(tokenFile);
        var token = JsonConvert.DeserializeObject<JObject>(tokenJson);
        if (string.IsNullOrWhiteSpace(token?.GetValue("access_token")?.ToString())) return false;
        AccessTokenSetter();
        return true;
    }

    private static void AccessTokenSetter()
    {
        var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var tokenFile = appDirectory + "Controllers/Files/tokens.json";
        var tokenJson = System.IO.File.ReadAllText(tokenFile);
        var token = JsonConvert.DeserializeObject<JObject>(tokenJson);
        if (!string.IsNullOrWhiteSpace(token?.GetValue("access_token")?.ToString()))
            _accessToken = token.GetValue("access_token")?.ToString();
        SetPrimaryCalendarId();
    }

    private static void SetPrimaryCalendarId()
    {
        while (true)
        {
            var credential = GoogleCredential.FromAccessToken(_accessToken);
            var service = new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                // ReSharper disable once StringLiteralTypo
                ApplicationName = "Softcon"
            });
            var calendarListEntry = service.CalendarList.Get("primary");
            try
            {
                var calendarList = calendarListEntry.Execute();
                _calendarId = calendarList.Id;
                return;
            }
            catch (GoogleApiException ex)
            {
                if (ex.HttpStatusCode == HttpStatusCode.Unauthorized) new OAuthController().RefreshToken();
            }
        }
    }

    // ReSharper disable once StringLiteralTypo
    [HttpPost("eventcreate")]
    public IActionResult EventCreate([FromBody] CalendarEvent eventData)
    {
        var calendarId = _calendarId;
        var credential = GoogleCredential.FromAccessToken(_accessToken);
        var service = new CalendarService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "My Calendar App"
        });
        var myEvent = new Event
        {
            Summary = eventData.Title,
            Location = eventData.Location,
            Description = eventData.Description,
            Start = new EventDateTime
            {
                DateTime = eventData.Start,
                TimeZone = eventData.TimeZone
            },
            End = new EventDateTime
            {
                DateTime = eventData.End,
                TimeZone = eventData.TimeZone
            },
            Reminders = new Event.RemindersData
            {
                UseDefault = true
            }
        };
        try
        {
            service.Events.Insert(myEvent, calendarId).Execute();
            return Json(new { success = true, message = "Event created successfully" });
        }
        catch (GoogleApiException ex)
        {
            if (ex.HttpStatusCode != HttpStatusCode.Unauthorized)
                return Json(new { success = false, message = "Error creating event" + ex.HttpStatusCode });
            new OAuthController().RefreshToken();
            var request = service.Events.Insert(myEvent, calendarId);
            request.Execute();
            return Json(new { success = true, message = "Event created successfully after refreshing token" });
        }
    }

    // ReSharper disable once StringLiteralTypo
    [HttpPost("recurringevent")]
    public IActionResult RecurringEvent([FromBody] RecurringEvent eventData)
    {
        var calendarId = _calendarId;
        var credential = GoogleCredential.FromAccessToken(_accessToken);
        var service = new CalendarService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "My Calendar App"
        });

        //converting the recurse until date time to utc and format that api accepts
        var dateTime = eventData.Until.ToUniversalTime();
        // ReSharper disable once StringLiteralTypo
        var dateTimeString = dateTime.ToString("yyyyMMddTHHmmssZ");

        //Creating a request to api call
        var myEvent = new Event
        {
            Summary = eventData.Title,
            Location = eventData.Location,
            Description = eventData.Description,
            Start = new EventDateTime
            {
                DateTime = eventData.Start,
                TimeZone = eventData.TimeZone
            },
            End = new EventDateTime
            {
                DateTime = eventData.End,
                TimeZone = eventData.TimeZone
            },
            Recurrence = new List<string>
            {
                //Compulsory Uppercase needed
                "RRULE:FREQ=" + eventData.RecurringOption?.ToUpper() + ";" +
                "UNTIL=" + dateTimeString + ";"
            },
            Reminders = new Event.RemindersData
            {
                UseDefault = true
            }
        };
        try
        {
            //sending a call
            service.Events.Insert(myEvent, calendarId).Execute();
            return Json(new { success = true, message = "Recurring event created successfully" });
        }
        catch (GoogleApiException ex)
        {
            if (ex.HttpStatusCode != HttpStatusCode.Unauthorized)
                return Json(new { success = false, message = "Error creating recurring event"});

            //in case of HttpStatusCode.Unauthorized it will refresh the token and retry
            new OAuthController().RefreshToken();
            var request = service.Events.Insert(myEvent, calendarId);
            request.Execute();
            return Json(new
                { success = true, message = "Recurring event created successfully after refreshing token" });
        }
    }
}