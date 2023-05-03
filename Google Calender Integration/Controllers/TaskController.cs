using System.Net;
using Google;
using Google_Calender_Integration.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Tasks.v1;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Google_Calender_Integration.Controllers;

[ApiController]
[Route("api/[controller]")]

public class TaskController : Controller
{
    private static string? _accessToken;
    private static string? _taskId;
    
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
    
    private static void SetPrimaryTaskId()
    {
        while (true)    
        {
            
            var credential = GoogleCredential.FromAccessToken(_accessToken);
            var service = new TasksService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                // ReSharper disable once StringLiteralTypo
                ApplicationName = "Softcon"
            });

            var taskListEntry = service.Tasklists.Get("@default");
            try
            {
                var taskList = taskListEntry.Execute();
                _taskId = taskList.Id;
                return;
            }
            catch (GoogleApiException ex)
            {
                if (ex.HttpStatusCode == HttpStatusCode.Unauthorized)
                {
                    new OAuthController().RefreshToken();
                }
            }
        }
    }

    private static void AccessTokenSetter()
    {
        var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var tokenFile = appDirectory + "Controllers/Files/tokens.json";
        var tokenJson = System.IO.File.ReadAllText(tokenFile);
        var token = JsonConvert.DeserializeObject<JObject>(tokenJson);
        if (!string.IsNullOrWhiteSpace(token?.GetValue("access_token")?.ToString()))
            _accessToken = token.GetValue("access_token")?.ToString();
        SetPrimaryTaskId();
    }
    // ReSharper disable once StringLiteralTypo
    [HttpPost("createtask")]
    public IActionResult CreateTask([FromBody] GoogleTask eventData)
    {
        var credential = GoogleCredential.FromAccessToken(_accessToken);
        var service = new TasksService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "My Calendar App"
        });
        var due = eventData.Due.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        /*
         Note: Currently the due date only records date information; the time portion of the timestamp is discarded 
         when setting the due date. It isn't possible to read or write the time that a task is due via the API.
         */
        var task = new Google.Apis.Tasks.v1.Data.Task
        {
            Title = eventData.Title,
            Notes = eventData.Notes,
            Due = due
        };

        try
        {
            service.Tasks.Insert(task, _taskId).Execute();
            return Json(new { success = true, message = "Task created successfully" });
        }
        catch (GoogleApiException ex)
        {
            if (ex.HttpStatusCode != HttpStatusCode.Unauthorized)
                return Json(new { success = false, message = "Error creating Task"});
            new OAuthController().RefreshToken();
            service.Tasks.Insert(task, _taskId).Execute();
            return Json(new { success = true, message = "Task created successfully after refreshing token" });
        }
    }
}