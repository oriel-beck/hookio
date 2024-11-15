using Hookio.DataManagers.Interfaces;

namespace Hookio.WebApi.Middlewares
{
    public class SessionRefresh(RequestDelegate next, IUserService userService)
    {
        private readonly RequestDelegate _next = next;
        private readonly IUserService _userService = userService;

        public async Task InvokeAsync(HttpContext context)
        {
            await _userService.ValidateSessionData(context.Session, CancellationToken.None);
            await _next(context);
        }
    }
    public static class RequestSessionRefreshExtensions
    {
        public static IApplicationBuilder UseSessionRefresh(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SessionRefresh>();
        }
    }
}
