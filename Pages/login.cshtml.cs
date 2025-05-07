using System.Data;
using System.Text;
using Factory;
using Factory.DB;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Model;
using Microsoft.AspNetCore.Http;
using System.Security.Policy;
using System.Text.Encodings.Web;
using Razor01.Global;
using Microsoft.AspNetCore.Http.Extensions;

public class LoginPage : PageModel
{
    private readonly ILogger<LoginPage> logger;
    private IDatabaseService db;
    public string Message { get; set; }
    public string ErrorMessage { get; set; }
    public string Domain { get; set; }
    public string RedirectURI {get;set;}
    public string OAuthURL { get; set; }

    public LoginPage(ILogger<LoginPage> _logger, IDatabaseService _db)
    {
        logger = _logger;
        db = _db;

        Domain = DomainHelper.GetPCDomainName();
    }

    public void OnGet()
    {
        RedirectURI = UrlEncoder
                        .Create()
                        .Encode(HttpContext.Request.GetDisplayUrl() + "OAuth");
        OAuthURL = $"{GlobalConfig.Instance.OAuthURL}/oauth/authorize?redirect_uri={RedirectURI}&client_id={GlobalConfig.Instance.OAuthClientId}&state=@state&nonce=@nonce";

        if (HttpContext.Session.Get("ASP_SessionID") != null)
        {
            Response.Redirect("/Home");
        }

        var nonce = Utility.GenerateNextNonce().ToString();
        var state = Guid.NewGuid().ToString();

        OAuthURL = OAuthURL
                        .Replace("@state", state)
                        .Replace("@nonce", nonce);

        Response.Cookies.Append("oauth-state", state);
        Response.Cookies.Append("oauth-nonce", nonce);

        if (Request.Query["err"].FirstOrDefault() == null)
        {
            Message = "Login to get started";
        }
        else
        {
            ErrorMessage = Request.Query["err"].First();
        }
    }

    public async Task OnPost()
    {
        try
        {
            var loginRequest = new LoginModel(db)
            {
                Domain = Request.Form["domain"],
                UserName = Request.Form["username"],
                Password = Request.Form["password"]
            };

            switch (await loginRequest.Login())
            {
                case LoginStatus.Pass:
                    logger.LogInformation($"User {loginRequest.UserName} successfully logged in.");

                    HttpContext.Session.SetString("ASP_SessionID", HttpContext.Session.Id);
                    HttpContext.Session.SetObject("User", loginRequest);

                    Response.Redirect("/Home");
                    break;
                case LoginStatus.Locked:
                    logger.LogError($"User {loginRequest.UserName} failed to log in. Account disabled.");

                    ErrorMessage = "Account has been locked";
                    break;
                default:
                    logger.LogError($"User {loginRequest.UserName} failed to log in. Invalid account.");

                    ErrorMessage = "Invalid Username/Password";
                    break;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }
}
