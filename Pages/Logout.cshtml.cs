using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Razor01.Pages;

public class LogoutPage : PageModel
{
    private readonly ILogger<LogoutPage> _logger;
    private readonly IDatabaseService _databaseService;

    public LogoutPage(ILogger<LogoutPage> logger, IDatabaseService databaseService)
    {
        _logger = logger;
        _databaseService = databaseService;
    }

    public void OnGet()
    {
        LogoutOAuth();

        if (HttpContext.Session.Get("ASP_SessionID") != null)
        {
            HttpContext.Session.Clear();
            foreach (var cookie in HttpContext.Request.Cookies)
            {
                Response.Cookies.Delete(cookie.Key);
            }
        }

        Response.Redirect("/");
    }

    private void LogoutOAuth()
    {

    }
}