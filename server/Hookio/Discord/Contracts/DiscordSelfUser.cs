
using Newtonsoft.Json;

namespace Hookio.Discord.Contracts
{
    public class DiscordSelfUser
    {
        public ulong Id { get; set; }
        public string Discriminator { get; set; } = "0";
        public string? Username { get; set; }
        public string? Avatar { get; set; }
        public string? GlobalName { get; set; }
        public string? Email { get; set; }
        [JsonConstructor]
        public DiscordSelfUser(ulong id, string discriminator, string? username, string? avatar, string? globalName, string? email)
        {
            Id = id;
            Discriminator = discriminator;
            Username = username;
            Avatar = avatar;
            GlobalName = globalName;
            Email = email;
        }

        public string GetAvatarUrl()
        {
            if (string.IsNullOrEmpty(Avatar))
            {
                // If the avatar is not set, return the default avatar URL based on the user's ID
                if (Discriminator != null && Discriminator == "0") return $"https://cdn.discordapp.com/embed/avatars/{(Id >> 22) % 6}.png";
                int discriminator = int.Parse(Discriminator ?? "0");
                return $"https://cdn.discordapp.com/embed/avatars/{discriminator % 5}.png";
            }
            else
            {
                // If the avatar is set, construct the avatar URL based on the hash
                if (Avatar.StartsWith("a_")) return $"https://cdn.discordapp.com/avatars/{Id}/{Avatar}.gif";
                return $"https://cdn.discordapp.com/avatars/{Id}/{Avatar}.png";
            }
        }
    }
}


