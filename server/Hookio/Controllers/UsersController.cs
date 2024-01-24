using Hookio.Contracts;
using Hookio.Database.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Hookio.Controllers
{
    [Route("/api/users")]
    [ApiController]
    public class UsersController(ILogger<UsersController> logger, IDataManager dataManager) : ControllerBase
    {
        private readonly HttpClient _http = new()
        {
            BaseAddress = new Uri("https://discord.com")
        };
        private readonly JwtSecurityTokenHandler _tokenHandler = new();

        [Authorize]
        [HttpGet("current")]
        public async Task<ActionResult<CurrentUserResponse>> GetCurrentUser()
        {
            var idClaim = HttpContext.User.Claims.First(claim => claim.Type == "id");
            return Ok(await dataManager.GetUser(idClaim.Value));
        }

        [HttpPost("authenticate/{code}")]
        public async Task<ActionResult<bool>> Authenticate(string code)
        {
            try
            {
                var discordResponse = await _http.PostAsync($"/api/v10/oauth2/token",
                new FormUrlEncodedContent(new Dictionary<string, string?>()
                {
                    { "code", code },
                    { "redirect_uri", "http://localhost:5173/login" },
                    { "grant_type", "authorization_code" },
                    { "client_id", Environment.GetEnvironmentVariable("CLIENT_ID")! },
                    { "client_secret", Environment.GetEnvironmentVariable("CLIENT_SECRET")! },
                    { "scopes", "identify guilds email" }
                }
                ));
                var result = await discordResponse.Content.ReadFromJsonAsync<DiscordTokenResponse>();

                _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {result?.AccessToken}");
                var user = await _http.GetFromJsonAsync<DiscordCurrentUserResponse>("/api/v10/users/@me");

                var claims = new Claim[]
                {
                    new("username", user!.UserName),
                    new("id", user.Id),
                    new("avatar", user.Avatar)
                };

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddHours(12),
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET")!)),
                        SecurityAlgorithms.HmacSha256Signature)
                };

                var token = _tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = _tokenHandler.WriteToken(token);
                // TODO: fix cookie auth
                HttpContext.Response.Cookies.Append("Authorization", tokenString, new CookieOptions
                {
                    //HttpOnly = true,
                    Expires = DateTime.UtcNow.AddHours(12),
                    SameSite = SameSiteMode.Strict
                });
                await dataManager.CreateUser(user, result);
                return Ok(new { Token = tokenString });
            } catch (Exception ex)
            {
                logger.LogError($"[DiscordCodeExchange] {ex.Message}");
                return Ok(false);
            }
        }
    }
}
