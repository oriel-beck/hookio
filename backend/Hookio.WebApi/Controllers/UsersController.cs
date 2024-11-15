using Discord.Rest;
using Hookio.Contracts.User;
using Hookio.DataManagers;
using Hookio.DataManagers.Interfaces;
using Hookio.Shared.Configuration;
using Hookio.Shared.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Web;

namespace Hookio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(
        IOptions<OAuth2> oauth2Options,
        IUserService userService
        ) : ControllerBase
    {
        private readonly IUserService _userService = userService;
        private readonly OAuth2 _oauth2Options = oauth2Options.Value;
        
        private Dictionary<string, string> QueryParamsDict => new() 
        {
            { "response_type", "code" },
            { "client_id", _oauth2Options.ClientId },
            { "redirect_uri", _oauth2Options.RedirectURI },
            { "scope", _oauth2Options.Scopes },
        };
        private string QueryParams => string.Join("&", QueryParamsDict
            .Where(pair => !string.IsNullOrEmpty(pair.Key) && !string.IsNullOrEmpty(pair.Value))
            .Select(pair => $"{Uri.EscapeDataString(pair.Key)}={HttpUtility.UrlEncode(pair.Value)}"));

        [HttpPost("[action]")]
        public async Task<ActionResult> Authenticate([FromQuery] string code, [FromQuery] string state, CancellationToken cancellationToken)
        {
            if (state != HttpContext.Session.GetWithExpiry<string>("State")) return Redirect(_oauth2Options.BaseURI);
            try
            {
                var result = await _userService.Authenticate(code, cancellationToken);
                if (result == null) return Redirect(_oauth2Options.BaseURI);

                var user = await _userService.GetRestUser(result.AccessToken, cancellationToken);
                if (user == null) return Redirect(_oauth2Options.BaseURI);

                List<Claim> claims = new() 
                {
                    { new Claim("Id", user.Id.ToString()) },
                };

                ClaimsIdentity identity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    AllowRefresh = true,
                    IssuedUtc = DateTime.UtcNow
                };

                await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(identity),
                        authProperties
                    );

                HttpContext.Session.SetWithExpiry("accessToken", result.AccessToken, TimeSpan.FromSeconds(result.ExpiresIn));
                await _userService.ValidateSessionData(HttpContext.Session, cancellationToken);

                return Redirect($"{_oauth2Options.BaseURI}/guilds");
            }
            catch (Exception)
            {
                return Redirect(_oauth2Options.BaseURI);
            }
        }

        [HttpGet("[action]")] 
        public ActionResult Login()
        {
            using RandomNumberGenerator rng = RandomNumberGenerator.Create();
            byte[] tokenData = new byte[32];
            rng.GetBytes(tokenData);

            string token = Convert.ToBase64String(tokenData);
            HttpContext.Session.SetWithExpiry("State", token, TimeSpan.FromMinutes(5));
            return Redirect($"https://discord.com/oauth2/authorize?{QueryParams}&state={token}&access_type=offline&prompt=none");
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult> LogOut()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            Response.Cookies.Append(".Hookio.Session", string.Empty, new CookieOptions
            {
                Expires = DateTime.UtcNow.AddDays(-1), // Expire immediately
                HttpOnly = true,
                Secure = true,
            });
            return Redirect(_oauth2Options.BaseURI);
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<CurrentUserResponse>> GetCurrentUser(CancellationToken cancellationToken)
        {
            await _userService.ValidateSessionData(HttpContext.Session, cancellationToken);
            var discordUser = HttpContext.Session.GetWithExpiry<RestSelfUser>("user");
            var guilds = HttpContext.Session.GetWithExpiry<List<RestUserGuild>>("guilds");
            return Ok(Contractor.ToContract(discordUser!, guilds));
        }
    }
}
