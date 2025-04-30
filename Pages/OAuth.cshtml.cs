using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;
using Model;
using Razor01.Global;

namespace Razor01.Pages;
public class OAuthPage : PageModel
{
    private readonly ILogger<OAuthPage> _logger;
    private readonly IDatabaseService _databaseService;
    private readonly UrlEncoder encoder = UrlEncoder.Create();
    private string OAuthWellKnownURI = "/.well-known/jwks.json";

    public OAuthPage(ILogger<OAuthPage> logger, IDatabaseService databaseService)
    {
        _logger = logger;
        _databaseService = databaseService;
    }

    public async Task OnGet()
    {
        var cookieState = Request.Cookies["oauth-state"];
        var cookieNonce = Request.Cookies["oauth-nonce"];

        var state = Request.Query["state"].First()?.ToString();
        if (state == null || state != cookieState)
        {
            var invalidMessage = encoder.Encode("OAuth: Invalid page state");
            Response.Redirect($"/?err={invalidMessage}");
            return;
        }

        var code = Request.Query["code"].First()?.ToString();
        if (code == null)
        {
            var invalidMessage = encoder.Encode("OAuth: Missing code");
            Response.Redirect($"/?err={invalidMessage}");
            return;
        }

        var codePlain = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(code));
        var codeNonce = codePlain.Split("..")[3];
        if (codeNonce != cookieNonce)
        {
            var invalidMessage = encoder.Encode("OAuth: Nonce mismatch");
            Response.Redirect($"/?err={invalidMessage}");
            return;
        }

        UserInfoResponse resp = null;
        LoginModel? loginDetail;
        var newAccount = false;
        try
        {
            resp = await GetUserInfo(code);
            loginDetail = LoginModel.GetUserByEmail(_databaseService, resp.email);
            newAccount = loginDetail.IsNew;
        }
        catch (InvalidDataException)
        {
            var invalidMessage = encoder.Encode("OAuth: Invalid authorization ID Token");
            Response.Redirect($"/?err={invalidMessage}");
            return;
        }
        catch (Exception)
        {
            newAccount = true;
            loginDetail = await LoginModel.RegisterForIDP(_databaseService, resp.email, code);
        }

        var loggedIn = await loginDetail.LoginViaIDP("oauth2", code);
        if (loggedIn == LoginStatus.Locked)
        {
            var accountLocked = encoder.Encode("OAuth: Account locked");
            Response.Redirect($"/?err={accountLocked}");
            return;
        }

        Response.Cookies.Append("oauth-code", code);
        Response.Cookies.Delete("oauth-state");

        if (newAccount)
        {
            Response.Redirect($"/Update?token={loginDetail.UserName}");
        }
        else
        {
            HttpContext.Session.SetString("ASP_SessionID", HttpContext.Session.Id);
            HttpContext.Session.SetObject("User", loginDetail);

            Response.Redirect("/Home");
        }
    }

    private async Task<UserInfoResponse> GetUserInfo(string code)
    {
        using (var httpClient = new HttpClient() { BaseAddress = new Uri(GlobalConfig.Instance.OAuthURL) })
        {
            StringContent content = new
            (
                $"code={code}",
                Encoding.UTF8,
                "application/x-www-form-urlencoded"
            );

            var response = await httpClient.PostAsync("/oauth/token", content);
            response.EnsureSuccessStatusCode();

            var token = await response.Content.ReadFromJsonAsync<TokenResponse>();
            var valid = await VerifyIDTokenAsync(token);

            if (!valid)
            {
                throw new InvalidDataException("Invalid ID Token");
            }

            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token.access_token}");

            var userInfoResponse = await httpClient.GetAsync("/userinfo");
            userInfoResponse.EnsureSuccessStatusCode();

            return await userInfoResponse.Content.ReadFromJsonAsync<UserInfoResponse>();
        }
    }

    private async Task<bool> VerifyIDTokenAsync(TokenResponse token)
    {
        var jsonWebKeySet = await GetJsonWebKeySetAsync();

        var handler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKeyResolver = (s, securityToken, identifier, parameters) => jsonWebKeySet.GetSigningKeys().Where(secKey => secKey.KeyId == identifier),
            ValidIssuer = GlobalConfig.Instance.OAuthURL,
            ValidAudience = GlobalConfig.Instance.OAuthClientId
        };

        try
        {
            // do nothing for claim principal for now
            var claimsPrincipal = handler.ValidateToken(token.id_token, validationParameters, out _);
            _logger.LogInformation("JWT token valid");
        }
        catch (SecurityTokenException)
        {
            _logger.LogError("JWT token invalid");
            return false;
        }

        return true;
    }

    private async Task<JsonWebKeySet> GetJsonWebKeySetAsync()
    {
        using (var httpClient = new HttpClient())
        {
            var json = await httpClient.GetStringAsync(GlobalConfig.Instance.OAuthURL + OAuthWellKnownURI);
            return new JsonWebKeySet(json);
        }
    }

    private class TokenResponse
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string id_token { get; set; }
    }

    private class UserInfoResponse
    {
        public string email { get; set; }
        public string sub { get; set; }
    }
}