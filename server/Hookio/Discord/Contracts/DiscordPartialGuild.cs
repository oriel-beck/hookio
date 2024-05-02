
using Newtonsoft.Json;

namespace Hookio.Discord.Contracts
{
    public class DiscordPartialGuild
    {
        public ulong Id { get; set; }
        public string? Name { get; set; }
        public string? Icon { get; set; }
        public bool Owner { get; set; }
        public ulong Permissions { get; set; }
        public string? IconUrl
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Icon)) return null; // Return null if no icon is set
                return $"https://cdn.discordapp.com/icons/{Id}/{Icon}.png";
            }
        }

        [JsonConstructor]
        public DiscordPartialGuild(ulong id, string? name, string? icon, bool owner, ulong permissions)
        {
            Id = id;
            Name = name;
            Icon = icon;
            Owner = owner;
            Permissions = permissions;
        }
    }
}
