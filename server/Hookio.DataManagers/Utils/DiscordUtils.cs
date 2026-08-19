using Hookio.Discord.Contracts;
using Hookio.Utils;
using System.Globalization;

namespace Hookio.DataManagers.Utils;

public static class DiscordUtils
{
    public static IEnumerable<DiscordEmbedPayload> ConvertEntityEmbedToDiscordEmbed(List<Database.Entities.Embed> embeds, TemplateHandler templateHandler)
    {
        return embeds.OrderBy(e => e.Index).Select(embed => ToPayload(embed, templateHandler));
    }

    private static DiscordEmbedPayload ToPayload(Database.Entities.Embed embed, TemplateHandler templateHandler)
    {
        var payload = new DiscordEmbedPayload
        {
            Title = NullIfEmpty(templateHandler.Parse(embed.Title)),
            Url = NullIfEmpty(templateHandler.Parse(embed.TitleUrl)),
            Description = NullIfEmpty(templateHandler.Parse(embed.Description)),
            Color = ParseColor(embed.Color),
            Author = ToAuthor(embed, templateHandler),
            Footer = ToFooter(embed, templateHandler),
            Image = ToMedia(templateHandler.Parse(embed.Image)),
            Thumbnail = ToMedia(templateHandler.Parse(embed.Thumbnail)),
            Fields = (embed.Fields ?? [])
                .OrderBy(f => f.Index)
                .Select(field => new DiscordEmbedField
                {
                    Name = templateHandler.Parse(field.Name) ?? "",
                    Value = templateHandler.Parse(field.Value) ?? "",
                    Inline = field.Inline
                })
                .Where(f => !string.IsNullOrEmpty(f.Name) && !string.IsNullOrEmpty(f.Value))
                .ToList()
        };

        if (embed.AddTimestamp)
        {
            payload.Timestamp = DateTime.UtcNow.ToString("o");
        }

        return payload;
    }

    private static DiscordEmbedAuthor? ToAuthor(Database.Entities.Embed embed, TemplateHandler templateHandler)
    {
        var name = NullIfEmpty(templateHandler.Parse(embed.Author));
        if (name is null) return null;
        return new DiscordEmbedAuthor
        {
            Name = name,
            Url = NullIfEmpty(templateHandler.Parse(embed.AuthorUrl)),
            IconUrl = NullIfEmpty(templateHandler.Parse(embed.AuthorIcon))
        };
    }

    private static DiscordEmbedFooter? ToFooter(Database.Entities.Embed embed, TemplateHandler templateHandler)
    {
        var text = NullIfEmpty(templateHandler.Parse(embed.Footer));
        if (text is null) return null;
        return new DiscordEmbedFooter
        {
            Text = text,
            IconUrl = NullIfEmpty(templateHandler.Parse(embed.FooterIcon))
        };
    }

    private static DiscordEmbedMedia? ToMedia(string? url)
    {
        url = NullIfEmpty(url);
        return url is null ? null : new DiscordEmbedMedia { Url = url };
    }

    private static int? ParseColor(string? color)
    {
        if (string.IsNullOrWhiteSpace(color)) return null;
        var hex = color.StartsWith('#') ? color[1..] : color;
        if (uint.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var value))
        {
            return (int)(value & 0xFFFFFF);
        }
        return null;
    }

    private static string? NullIfEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
}
