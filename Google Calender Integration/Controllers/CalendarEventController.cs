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

    private readonly MessageModel _messageModel = new();
    //Function to set value of _accessToken
    // ReSharper disable once StringLiteralTypo
    [HttpGet("hasaccesstoken")]
    public MessageModel HasAccessToken()
    {
        var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var tokenFile = appDirectory + "Controllers/Files/tokens.json";
        if (!System.IO.File.Exists(tokenFile))
        {
            _messageModel.Status = "Failed";
            _messageModel.Message = "Token File Does not not exist";
            return _messageModel;
        }
        var tokenJson = System.IO.File.ReadAllText(tokenFile);
        var token = JsonConvert.DeserializeObject<JObject>(tokenJson);
        if (string.IsNullOrWhiteSpace(token?.GetValue("access_token")?.ToString()))
        {
            _messageModel.Status = "Failed";
            _messageModel.Message = "Token Not Available";
            return _messageModel;
        }
        AccessTokenSetter();
        _messageModel.Status = "Success";
        _messageModel.Message = "Token Available";
        return _messageModel;
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

    private static void SetPrimaryCalendarId() // This method can be used to get any calendar id primary or any other by changing below "primary" with variable
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
    public MessageModel EventCreate([FromBody] CalendarEvent eventData)
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
            _messageModel.Status = "Success";
            _messageModel.Message = "Event created successfully";
            return _messageModel;
        }
        catch (GoogleApiException ex)
        {
            if (ex.HttpStatusCode != HttpStatusCode.Unauthorized)
            {
                _messageModel.Status = "Failed";
                _messageModel.Message = "Error creating event";
                return _messageModel;
            }
            new OAuthController().RefreshToken();
            var request = service.Events.Insert(myEvent, calendarId);
            request.Execute();
            _messageModel.Status = "Success";
            _messageModel.Message = "Event created successfully after refreshing token";
            return _messageModel;
        }
    }

    // ReSharper disable once StringLiteralTypo
    [HttpPost("recurringevent")]
    public MessageModel RecurringEvent([FromBody] RecurringEvent eventData)
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
            _messageModel.Status = "Success";
            _messageModel.Message = "Event created successfully";
            return _messageModel;
        }
        catch (GoogleApiException ex)
        {
            if (ex.HttpStatusCode != HttpStatusCode.Unauthorized)
            {
                _messageModel.Status = "Failed";
                _messageModel.Message = "Error creating recurring event";
                return _messageModel;
            }
            //in case of HttpStatusCode.Unauthorized it will refresh the token and retry
            new OAuthController().RefreshToken();
            var request = service.Events.Insert(myEvent, calendarId);
            request.Execute();
            _messageModel.Status = "Success";
            _messageModel.Message = "Event created successfully after refreshing token";
            return _messageModel;
        }
    }
}