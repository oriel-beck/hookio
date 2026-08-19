using Hookio.Shared;
using System.Security.Claims;
using System.Text.Json;

namespace Hookio
{
    public static class Util
    {
        public static CookieOptions AuthCookieOptions(HttpContext context, DateTimeOffset expires)
        {
            var secureEnv = string.Equals(Environment.GetEnvironmentVariable(EnvNames.CookieSecure), "true", StringComparison.OrdinalIgnoreCase);
            return new CookieOptions
            {
                HttpOnly = true,
                Expires = expires,
                SameSite = SameSiteMode.Strict,
                Secure = context.Request.IsHttps || secureEnv
            };
        }

        public static bool CanAccessGuild(ClaimsPrincipal user, ulong guildId)
        {
            var userGuildsClaim = user.Claims.FirstOrDefault(claim => claim.Type == AuthConstants.GuildsClaim);
            if (userGuildsClaim is null) return false;
            var guilds = JsonSerializer.Deserialize<List<string>>(userGuildsClaim.Value);
            return guilds is not null && guilds.Contains(guildId.ToString());
        }
    }
}
