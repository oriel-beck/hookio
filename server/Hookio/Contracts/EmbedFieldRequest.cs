﻿using Discord;
using Microsoft.AspNetCore.Components.Routing;
using System.ComponentModel.DataAnnotations;

namespace Hookio.Contracts
{
    public class EmbedFieldRequest
    {
        [Required]
        [MaxLength(256)]
        public required string Name { get; set; }
        [Required]
        [MaxLength(1024)]
        public required string Value { get; set; }
        public bool Inline { get; set; }
        public int Length
        {
            get
            {
                return Name.Length + Value.Length;
            }
        }
    }
}
