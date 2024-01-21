using Hookio.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Hookio.Controllers
{
    [Route("/api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly HttpClient _http = new()
        {
            BaseAddress = new Uri("https://discord.com")
        };

        [HttpGet("current")]
        public IActionResult GetCurrentUser()
        {
            return Ok(new CurrentUserResponse());
        }

        [HttpGet("servers")]
        public IActionResult GetUserServers() 
        {
            return Ok(new List<string>());
        }

        [HttpPost("authenticate/{code}")]
        public async Task<IActionResult> Authenticate(string code)
        {
            var discordResponse = await _http.PostAsync($"/api/v10/oauth2/token", 
                new FormUrlEncodedContent(new Dictionary<string, string?>()
                {
                    { "code", code },
                    { "redirect_uri", "http://localhost:5173/login" },
                    { "grant_type", "authorization_code" },
                    { "client_id", Environment.GetEnvironmentVariable("CLIENT_ID")! },
                    { "client_secret", Environment.GetEnvironmentVariable("CLIENT_SECRET")! },
                    { "scopes", "identify guilds" }
                }
                ));
            Console.Write(await discordResponse.Content.ReadFromJsonAsync<dynamic>());
            return Ok(code);
        }
    }
}
