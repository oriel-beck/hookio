﻿namespace Hookio.Contracts
{
    public class CurrentUserResponse
    {
        public required ulong Id { get; set; }
        public string? Username { get; set; }
        public string Discriminator { get; set; } = "0";
        public int Premium { get; set; } = 0;
        public string? Avatar { get; set; }
        public required IEnumerable<GuildResponse> Guilds { get; set; }
    }
}
