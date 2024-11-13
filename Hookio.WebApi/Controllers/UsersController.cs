using Hookio.DataManagers.Interfaces;
using Hookio.Shared.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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
        public async Task<IActionResult> Authenticate([FromQuery] string code, [FromQuery] string state, CancellationToken cancellationToken)
        {
            if (state != HttpContext.Session.GetString("State")) return Redirect(_oauth2Options.BaseURI);
            try
            {
                var result = await _userService.Authenticate(code, cancellationToken);
                if (result == null) return Redirect(_oauth2Options.BaseURI);
                return Redirect($"{_oauth2Options.BaseURI}/guilds");
            }
            catch (Exception)
            {
                return Redirect(_oauth2Options.BaseURI);
            }
        }

        [HttpGet("[action]")] 
        public IActionResult Login()
        {
            using RandomNumberGenerator rng = RandomNumberGenerator.Create();
            byte[] tokenData = new byte[32];
            rng.GetBytes(tokenData);

            string token = Convert.ToBase64String(tokenData);
            HttpContext.Session.SetString("State", token);
            return Redirect($"https://discord.com/oauth2/authorize?{QueryParams}&state={token}&access_type=offline&prompt=none");
        }
    }
}
