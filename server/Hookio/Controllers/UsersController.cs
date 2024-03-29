﻿using Discord.Rest;
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
                    { "redirect_uri", "http://localhost:5173/" },
                    { "grant_type", "authorization_code" },
                    { "client_id", Environment.GetEnvironmentVariable("DISCORD_CLIENT_ID")! },
                    { "client_secret", Environment.GetEnvironmentVariable("DISCORD_CLIENT_SECRET")! },
                    { "scopes", "identify guilds email" }
                }
                ));
                var result = await discordResponse.Content.ReadFromJsonAsync<OAuth2ExchangeResponse>();
                if (result == null)
                {
                    logger.LogInformation($"[{nameof(Authenticate)}]: Failed to code exchange with code '{code}'");
                    return Ok(false);
                }

                var client = new DiscordRestClient();
                await client.LoginAsync(Discord.TokenType.Bearer, result.AccessToken);

                var user = client.CurrentUser;
                user ??= await client.GetCurrentUserAsync();

                await dataManager.CreateUser(client, result);
                var currentUser = await dataManager.GetUser(user.Id);

                var claims = new Claim[]
                {
                    new("id", user.Id.ToString()),
                    new("guilds", JsonConvert.SerializeObject(currentUser?.Guilds.Select(g => g.Id)))
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
                HttpContext.Response.Cookies.Append("Authorization", tokenString, new()
                {
                    HttpOnly = true,
                    Expires = DateTime.UtcNow.AddHours(12),
                    SameSite = SameSiteMode.Strict
                });
                return Ok(currentUser);
            } catch (Exception ex)
            {
                logger.LogError($"[{nameof(Authenticate)}]: '{ex.Message}'");
                return Ok(false);
            }
        }
    }
}
