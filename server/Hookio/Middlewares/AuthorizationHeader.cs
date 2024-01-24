using Newtonsoft.Json;

namespace Hookio.Middlewares
{
public class AuthorizationHeader
{
    private readonly RequestDelegate _next;

    public AuthorizationHeader(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var authenticationCookieName = "hookio-auth";
        var cookie = context.Request.Cookies[authenticationCookieName];
        if (cookie != null)
        {

            if (!context.Request.Path.ToString().Contains("/logout", StringComparison.CurrentCultureIgnoreCase))
            {
                if (!string.IsNullOrEmpty(cookie))
                {
                        var headerValue = cookie;
                        if (context.Request.Headers.ContainsKey("Authorization"))
                        {
                            context.Request.Headers["Authorization"] = headerValue;
                        }else
                        {
                            context.Request.Headers.Append("Authorization", headerValue);
                        }
                }
                await _next.Invoke(context);
            }
            else
            {
                // this is a logout request, clear the cookie by making it expire now
                context.Response.Cookies.Append(authenticationCookieName,
                                                "",
                                                new CookieOptions()
                                                {
                                                    Path = "/",
                                                    HttpOnly = true,
                                                    Secure = false,
                                                    Expires = DateTime.UtcNow.AddHours(-1)
                                                });
                context.Response.Redirect("/login");
                return;
            }
        }
        else
        {
            await _next.Invoke(context);
        }
    }
}
}
