﻿
using Newtonsoft.Json;

namespace Hookio.Discord.Contracts
{
    [method: JsonConstructor]
    public class DiscordSelfUser(ulong id, string discriminator, string? username, string? avatar, string? globalName, string? email)
    {
        public ulong Id { get; } = id;
        public string Discriminator { get; } = discriminator;
        public string? Username { get; } = username;
        public string? Avatar { get; } = avatar;
        public string? GlobalName { get; } = globalName;
        public string? Email { get; } = email;

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


