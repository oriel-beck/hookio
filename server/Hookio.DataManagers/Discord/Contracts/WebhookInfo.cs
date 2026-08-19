using System.Text.Json.Serialization;

namespace Hookio.Discord.Contracts;

/// <summary>
/// Discord webhook object from GET /webhooks/{id}/{token}.
/// Snowflakes are strings per https://discord.com/developers/docs/resources/webhook#webhook-object
/// </summary>
public class WebhookInfo
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("channel_id")]
    public string? ChannelId { get; set; }

    [JsonPropertyName("guild_id")]
    public string? GuildId { get; set; }
}
