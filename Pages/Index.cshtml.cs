using Microsoft.AspNetCore.Mvc.RazorPages;
using Model;
using System.Text.Json;

namespace Razor01.Pages;

public class IndexPage : PageModel
{
    private readonly ILogger<IndexPage> _logger;
    private readonly IDatabaseService _databaseService;
    public LoginModel SignInUser { get; set; }
    public string UserInfo { get; set; }

    public IndexPage(ILogger<IndexPage> logger, IDatabaseService databaseService)
    {
        _logger = logger;
        _databaseService = databaseService;
    }

    public void OnGet()
    {
        if (HttpContext.Session.Get("ASP_SessionID") == null)
        {
            Response.Redirect("/");
        }

        SignInUser = HttpContext.Session.GetObject<LoginModel>("User");
        UserInfo = JsonSerializer.Serialize(SignInUser);
    }
}
