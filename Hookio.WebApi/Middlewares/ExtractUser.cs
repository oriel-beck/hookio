using Hookio.DataManagers.Interfaces;

namespace Hookio.WebApi.Middlewares
{
    public class ExtractUser(RequestDelegate next, IUserService userService)
    {
        private readonly RequestDelegate _next = next;
        private readonly IUserService _userService = userService;

        public async Task InvokeAsync(HttpContext context)
        {
            var userId = context.User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
            if (userId != null && ulong.TryParse(userId, out var ulongUserId))
            {
                var user = await _userService.GetUser(ulongUserId, CancellationToken.None);
                context.Items.Add("User", user);
            }
            await _next(context);
        }
    }

    public static class RequestExtractUserExtensions
    {
        public static IApplicationBuilder UseExtractUser(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExtractUser>();
        }
    }
}
