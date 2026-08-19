using Hookio.Shared;
using Microsoft.AspNetCore.Http;

namespace Hookio.Database;

internal static class UtilCookie
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
}
