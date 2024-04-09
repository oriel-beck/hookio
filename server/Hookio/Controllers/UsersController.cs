using Discord.Rest;
using Hookio.Contracts;
using Hookio.Database.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
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
            ulong.TryParse(idClaim.Value, out var userId);
            return Ok(await dataManager.GetUser(userId));
        }

        [HttpPost("logout")]
        public IActionResult LogOut()
        {
            HttpContext.Response.Cookies.Append("Authorization", "", new()
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow,
                SameSite = SameSiteMode.Strict
            });
            return NoContent();
        }


        [HttpPost("authenticate/{code}")]
        public async Task<ActionResult<CurrentUserResponse>> Authenticate(string code)
        {
            try
            {
                var discordResponse = await _http.PostAsync($"/api/v10/oauth2/token",
                new FormUrlEncodedContent(new Dictionary<string, string?>()
                {
                    { "code", code },
                    { "redirect_uri", Environment.GetEnvironmentVariable("DISCORD_REDIRECT_URI") },
                    { "grant_type", "authorization_code" },
                    { "client_id", Environment.GetEnvironmentVariable("DISCORD_CLIENT_ID")! },
                    { "client_secret", Environment.GetEnvironmentVariable("DISCORD_CLIENT_SECRET")! },
                    { "scopes", "identify guilds email" }
                }
                ));
                var result = await discordResponse.Content.ReadFromJsonAsync<OAuth2ExchangeResponse>();
                if (result == null)
                {
                    logger.LogInformation("[{FunctionName}]: Failed to code exchange with code '{Scopes}'", nameof(Authenticate), code);
                    return Ok(false);
                }

                if (!result.Scope.Contains("email") || !result.Scope.Contains("identify") || !result.Scope.Contains("guilds"))
                {
                    logger.LogInformation("[{FunctionName}]: Did not get all required scopes, cancelled login, got scopes '{Scopes}'", nameof(Authenticate), result.Scope);
                    return Ok(false);
                }

                var client = new DiscordRestClient();
                await client.LoginAsync(Discord.TokenType.Bearer, result.AccessToken);

                var user = client.CurrentUser;
                user ??= await client.GetCurrentUserAsync();

                await dataManager.CreateUser(client, result);
                var currentUser = await dataManager.GetUser(user.Id);
                
                CreateTokenAndSetCookie(_tokenHandler, HttpContext, user, currentUser?.Guilds.Select(g => g.Id));

                return Ok(currentUser);
            }
            catch (Exception ex)
            {
                logger.LogError($"[{nameof(Authenticate)}]: '{ex.Message}'");
                return Ok(false);
            }
        }

        internal static void CreateTokenAndSetCookie(JwtSecurityTokenHandler tokenHandler, HttpContext context, RestSelfUser user, IEnumerable<string>? guildIds)
        {
            var claims = new Claim[]
                            {
                    new("id", user.Id.ToString()),
                    new("guilds", JsonConvert.SerializeObject(guildIds))
             };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(12),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET")!)),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            context.Response.Cookies.Append("Authorization", tokenString, new()
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddHours(12),
                SameSite = SameSiteMode.Strict
            });
        }
    }
}
