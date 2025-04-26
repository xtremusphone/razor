using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Model;

namespace Razor01.Pages;

public class UpdatePage : PageModel
{
    private readonly ILogger<UpdatePage> _logger;
    private readonly IDatabaseService _databaseService;
    public string UID { get; set; }

    public UpdatePage(ILogger<UpdatePage> logger, IDatabaseService databaseService)
    {
        _logger = logger;
        _databaseService = databaseService;
    }

    public async Task OnGet()
    {
        UID = Request.Query["token"];

        if (UID == null)
        {
            var invalidState = UrlEncoder.Create().Encode("Update: Invalid request");
            Response.Redirect($"/?err={invalidState}");
        }
    }

    public async Task OnPost()
    {
        var existingUsername = Request.Form["UID"];
        var newUsername = Request.Form["username"];

        var user = LoginModel.GetUserByUsername(_databaseService, existingUsername);

        await user.UpdateNewUserAsync(newUsername);

        HttpContext.Session.SetString("ASP_SessionID", HttpContext.Session.Id);
        HttpContext.Session.SetObject("User", user);

        Response.Redirect("/Home");
    }
}