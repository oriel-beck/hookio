﻿namespace Hookio.Contracts
{
    public class MessageRequest
    {
        public string Content { get; set; } = string.Empty;
        public EmbedRequest[] Embeds { get; set; } = [];
        public string? Username { get; set; }
        // TODO: pass avatar file, convert to base64 and save in db for now.
        //public byte[]? Avatar { get; set; }
    }
}