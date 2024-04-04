using Hookio.Contracts;
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

        public static int GetEmbedLength(Database.Entities.Embed embed)
        {
            int num = embed.Title?.Length ?? 0;
            int valueOrDefault = (embed.Author?.Length).GetValueOrDefault();
            int num2 = embed.Description?.Length ?? 0;
            int valueOrDefault2 = (embed.Footer?.Length).GetValueOrDefault();
            return num + valueOrDefault + num2 + valueOrDefault2;
        }

        public static int GetEmbedLength(EmbedRequest embed)
        {
            int num = embed.Title?.Length ?? 0;
            int valueOrDefault = (embed.Author?.Length).GetValueOrDefault();
            int num2 = embed.Description?.Length ?? 0;
            int valueOrDefault2 = (embed.Footer?.Length).GetValueOrDefault();
            int valueOrDefault3 = embed.Fields.Sum((EmbedFieldRequest f) => f.Name?.Length + f.Value?.ToString().Length).GetValueOrDefault();
            return num + valueOrDefault + num2 + valueOrDefault2;
        }
    }
}
