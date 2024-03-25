using Newtonsoft.Json;
using System.Security.Claims;

namespace Hookio
{
    public static class Util
    {
        public static bool CanAccessGuild(ClaimsPrincipal user, ulong guildId)
        {
            var userGuildsClaim = user.Claims.First((claim) => claim.Type == "guilds");
            var guilds = JsonConvert.DeserializeObject<List<ulong>>(userGuildsClaim.Value);
            return guilds!.Contains(guildId);
        }
    }
}
