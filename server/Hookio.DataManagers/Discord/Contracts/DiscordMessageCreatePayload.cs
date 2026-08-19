using System.Text.Json.Serialization;

namespace Hookio.Discord.Contracts;

/// <summary>
/// Execute/edit webhook JSON per https://discord.com/developers/docs/resources/webhook#execute-webhook
/// </summary>
public class DiscordMessageCreatePayload
{
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("avatar_url")]
    public string? AvatarUrl { get; set; }

    [JsonPropertyName("embeds")]
    public IEnumerable<DiscordEmbedPayload>? Embeds { get; set; }
}

public class DiscordEmbedPayload
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("color")]
    public int? Color { get; set; }

    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; set; }

    [JsonPropertyName("author")]
    public DiscordEmbedAuthor? Author { get; set; }

    [JsonPropertyName("footer")]
    public DiscordEmbedFooter? Footer { get; set; }

    [JsonPropertyName("image")]
    public DiscordEmbedMedia? Image { get; set; }

    [JsonPropertyName("thumbnail")]
    public DiscordEmbedMedia? Thumbnail { get; set; }

    [JsonPropertyName("fields")]
    public IEnumerable<DiscordEmbedField>? Fields { get; set; }
}

public class DiscordEmbedAuthor
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("icon_url")]
    public string? IconUrl { get; set; }
}

public class DiscordEmbedFooter
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = "";

    [JsonPropertyName("icon_url")]
    public string? IconUrl { get; set; }
}

public class DiscordEmbedMedia
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = "";
}

public class DiscordEmbedField
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("value")]
    public string Value { get; set; } = "";

    [JsonPropertyName("inline")]
    public bool Inline { get; set; }
}
