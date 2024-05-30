
using Newtonsoft.Json;

namespace Hookio.Discord.Contracts
{
    [method: JsonConstructor]
    public class DiscordPartialGuild(ulong id, string? name, string? icon, bool owner, ulong permissions)
    {
        public ulong Id { get; } = id;
        public string? Name { get; } = name;
        public string? Icon { get; } = icon;
        public bool Owner { get; } = owner;
        public ulong Permissions { get; } = permissions;
        public string? IconUrl
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Icon)) return null; // Return null if no icon is set
                return $"https://cdn.discordapp.com/icons/{Id}/{Icon}.png";
            }
        }
    }
}
