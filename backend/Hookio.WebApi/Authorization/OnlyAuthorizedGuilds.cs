using Hookio.Contracts.Discord;
using Hookio.Shared.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Hookio.WebApi.Authorization
{
    [AttributeUsage(AttributeTargets.All)]
    public class OnlyAuthorizedGuilds(string routeParameterName) : Attribute, IAuthorizationFilter
    {
        private readonly string _routeParameterName = routeParameterName;

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var httpContext = context.HttpContext;

            // Access route parameters
            var guildId = httpContext?.GetRouteValue(_routeParameterName)?.ToString();

            // Access session data
            var guilds = httpContext?.Session.GetWithExpiry<List<DiscordGuild>>("guilds");

            // Authorization logic: check if the guild ID exists in the user's guild array
            if (guildId == null || guilds == null)
            {
                context.Result = new ForbidResult(); // Authorization failed
                return;
            }

            if (!guilds.Any(g => g.Id == guildId))
            {
                context.Result = new ForbidResult(); // Authorization failed
                return;
            }

            // Authorization succeeded (do nothing, the request continues)
        }
    }
}
