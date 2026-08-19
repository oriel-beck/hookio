using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Hookio.Shared;
using Microsoft.IdentityModel.Tokens;

namespace Hookio.Tests;

internal static class TestJwt
{
    public const string Secret = "hookio-test-jwt-secret-key-32bytes-min";

    public static string Create(string userId, params string[] guildIds)
    {
        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(AuthConstants.IdClaim, userId),
                new Claim(AuthConstants.GuildsClaim, System.Text.Json.JsonSerializer.Serialize(guildIds))
            ]),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = AuthConstants.Issuer,
            Audience = AuthConstants.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret)),
                SecurityAlgorithms.HmacSha256Signature)
        });
        return handler.WriteToken(token);
    }
}
