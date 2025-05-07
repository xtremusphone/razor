using Microsoft.AspNetCore.Mvc.RazorPages;
using Model;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Model;
using Razor01.Global;
using System.Text.Json;
using System.Threading.Tasks;

namespace Razor01.Pages;

public class IndexPage : PageModel
{
    private readonly ILogger<IndexPage> _logger;
    private readonly ILogger<RentasPlusApi> _apilogger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IDatabaseService _databaseService;
    public LoginModel SignInUser { get; set; }
    public string UserInfo { get; set; }
    public string Message { get; set; }

    public IndexPage(ILogger<IndexPage> logger, IDatabaseService databaseService, ILogger<RentasPlusApi> apilogger, ILoggerFactory loggerFactory)
    {
        _logger = logger;
        _databaseService = databaseService;
        _apilogger = apilogger;
        _loggerFactory = loggerFactory;
    }

    public async Task OnGet()
    {
        if (HttpContext.Session.Get("ASP_SessionID") == null)
        {
            Response.Redirect("/");
        }

        SignInUser = HttpContext.Session.GetObject<LoginModel>("User");
        UserInfo = JsonSerializer.Serialize(SignInUser);

        var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(GlobalConfig.Instance.RPNRSSURL);

        var api = new RentasPlusApi(_apilogger, _loggerFactory, httpClient, new Org.OpenAPITools.Client.JsonSerializerOptionsProvider(JsonSerializerOptions.Web), new RentasPlusApiEvents());
        var request = new AccountBalanceListRequest();
        request.Source = "";
        request.Timestamp = DateTime.Now.Microsecond;
        request.Reference = DateTime.Now.ToLongTimeString();

        var response = await api.ApiRentasPlusAccountBalancePostAsync(request);
        if (response.IsOk)
        {
            Message = "Successfully called RPNRSS API";
        }
        else
        {
            Message = "Failled to call RPNRSS API";
        }
    }
}
