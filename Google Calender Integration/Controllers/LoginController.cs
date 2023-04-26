using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace Google_Calender_Integration.Controllers;

[ApiController]
[Route("api/[controller]")]


public class LoginController : Controller
{
    [HttpGet]
    public ActionResult OauthRedirect()
    {
        //File location for the the json
        var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var credentialsFile = appDirectory + "Controllers/Files/credentials.json";
        //Reading the file
        var credentials = JObject.Parse(System.IO.File.ReadAllText(credentialsFile));
        var clientId = credentials["client_id"];
        //Same as in api console
        const string redirectUri = "https://localhost:44416/api/oauth/callback";
        //Creating a URL for initial api call to get code
        var redirectUrl = "https://accounts.google.com/o/oauth2/v2/auth?" +
                          "scope=https://www.googleapis.com/auth/calendar+https://www.googleapis.com/auth/calendar.events&" +
                          "access_type=offline&" +
                          "include_granted_scopes=true&" +
                          "response_type=code&" +
                          // ReSharper disable once StringLiteralTypo
                          "state=hellothere&" +
                          "redirect_uri="+ redirectUri +"&" +
                          "client_id=" + clientId;
        return Redirect(redirectUrl);
    }
}