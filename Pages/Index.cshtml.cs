using Factory.DB.Init;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Razor01.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IDatabaseService _databaseService;

    public IndexModel(ILogger<IndexModel> logger, IDatabaseService databaseService)
    {
        _logger = logger;
        _databaseService = databaseService;
    }

    public void OnGet()
    {
        var initDB = new InitDB("Razor01", _databaseService);
    }
}
