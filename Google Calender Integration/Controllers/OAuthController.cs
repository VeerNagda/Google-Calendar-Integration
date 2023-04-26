using System.Net;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace ASP.NETCoreWebApplication1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OAuthController : Controller
{
    //sub url of controller
    [HttpGet("callback")]
    public void Callback(string code, string state, string error = null)
    {   
        //checks of error and in, if none, calls the function
        if (string.IsNullOrWhiteSpace(error)) GetTokens(code);
    }

    public void GetTokens(string code)
    {   
        //loading files
        var tokenFile =
            "D:/ASP/git/ASP.NETCoreWebApplication1/ASP.NETCoreWebApplication1/Controllers/Files/tokens.json";
        var credentialsFile =
            "D:/ASP/git/ASP.NETCoreWebApplication1/ASP.NETCoreWebApplication1/Controllers/Files/credentials.json";
        var credentials = JObject.Parse(System.IO.File.ReadAllText(credentialsFile));

        var restClient = new RestClient();
        var request = new RestRequest();
        
        //creating a url for getting token 
        request.AddQueryParameter("client_id", credentials["client_id"].ToString());
        request.AddQueryParameter("client_secret", credentials["client_secret"].ToString());
        request.AddQueryParameter("code", code);
        request.AddQueryParameter("grant_type", "authorization_code");
        request.AddQueryParameter("redirect_uri", "https://localhost:44450/api/oauth/callback");
        restClient.BaseUrl = new Uri("https://oauth2.googleapis.com/token");
        var response = restClient.Post(request);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            //saving token in the file
            System.IO.File.WriteAllText(tokenFile, response.Content);
            Response.Redirect("https://localhost:44450/");
        }
        Response.Redirect("https://localhost:44450/login");
    }

    [HttpGet("refreshtoken")]
    public void RefreshToken()
    {
        var tokenFile = 
            "D:/ASP/git/ASP.NETCoreWebApplication1/ASP.NETCoreWebApplication1/Controllers/Files/tokens.json";
        var credentialsFile = 
            "D:/ASP/git/ASP.NETCoreWebApplication1/ASP.NETCoreWebApplication1/Controllers/Files/credentials.json";
        var credentials = JObject.Parse(System.IO.File.ReadAllText(credentialsFile));
        var tokens = JObject.Parse(System.IO.File.ReadAllText(tokenFile));

        var restClient = new RestClient();
        var request = new RestRequest();

        request.AddQueryParameter("client_id", credentials["client_id"].ToString());
        request.AddQueryParameter("client_secret", credentials["client_secret"].ToString());
        request.AddQueryParameter("grant_type", "refresh_token");
        request.AddQueryParameter("refresh_token", tokens["refresh_token"].ToString());

        restClient.BaseUrl = new Uri("https://oauth2.googleapis.com/token");
        var response = restClient.Post(request);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var newTokens = JObject.Parse(response.Content);
            newTokens["refresh_token"] = tokens["refresh_token"].ToString();
            System.IO.File.WriteAllText(tokenFile, newTokens.ToString());
            //TODO create a response
        }
        Response.Redirect("https://localhost:44450/login");

    }

    [HttpGet("revoketoken")]
    public void RevokeToken()
    {
        var tokenFile = 
            "D:/ASP/git/ASP.NETCoreWebApplication1/ASP.NETCoreWebApplication1/Controllers/Files/tokens.json";
        var tokens = JObject.Parse(System.IO.File.ReadAllText(tokenFile));

        var restClient = new RestClient();
        var request = new RestRequest();

        request.AddQueryParameter("token", tokens["access_token"].ToString());

        restClient.BaseUrl = new Uri("https://oauth2.googleapis.com/revoke");
        var response = restClient.Post(request);
        Console.WriteLine(response.StatusCode); 
        if (response.StatusCode == HttpStatusCode.OK)
        {
            Response.Redirect("https://localhost:44450/login");
        }

    }
}