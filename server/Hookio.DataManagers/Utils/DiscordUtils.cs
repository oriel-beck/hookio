﻿using Discord;
using Hookio.Utils;
using System.Globalization;

namespace Hookio.DataManagers.Utils
{
    public static class DiscordUtils
    {
        public static IEnumerable<Embed> ConvertEntityEmbedToDiscordEmbed(List<Database.Entities.Embed> embeds, TemplateHandler templateHandler)
        {
            return embeds.Select((embed) =>
            {
                var embedBuilder = new EmbedBuilder()
                    .WithTitle(templateHandler.Parse(embed.Title ?? ""))
                    .WithUrl(templateHandler.Parse(embed.TitleUrl ?? ""))
                    .WithDescription(templateHandler.Parse(embed.Description ?? ""))
                    .WithImageUrl(templateHandler.Parse(embed.Image ?? ""))
                    .WithThumbnailUrl(templateHandler.Parse(embed.Thumbnail ?? ""))
                    .WithFooter(builder => builder.WithText(templateHandler.Parse(embed.Footer ?? "")).WithIconUrl(templateHandler.Parse(embed.FooterIcon ?? "")))
                    .WithAuthor(builder => builder.WithName(templateHandler.Parse(embed.Author ?? "")).WithUrl(templateHandler.Parse(embed.AuthorUrl ?? "")).WithIconUrl(templateHandler.Parse(embed.AuthorIcon ?? "")))
                    .WithFields(ConvertEntityEmbedFieldToEmbedField(embed.Fields, templateHandler));
                if (embed.AddTimestamp) embedBuilder.WithCurrentTimestamp();
                if (!string.IsNullOrEmpty(embed.Color))
                {
                    uint decValue = uint.Parse(embed.Color[1..], NumberStyles.HexNumber);
                    embedBuilder.Color = new Color(decValue);
                }
                return embedBuilder.Build();
            });
        }

        private static IEnumerable<EmbedFieldBuilder> ConvertEntityEmbedFieldToEmbedField(List<Database.Entities.EmbedField> embedFields, TemplateHandler templateHandler)
        {
            return embedFields.Select((field) => new EmbedFieldBuilder().WithName(templateHandler.Parse(field.Name ?? "")).WithValue(templateHandler.Parse(field.Value ?? "")).WithIsInline(field.Inline));
        }
    }
}
