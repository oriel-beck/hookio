using System.Text.Json;
using Hookio.Discord;
using Hookio.Discord.Contracts;

namespace Hookio.Tests;

public class DiscordJsonTests
{
    [Fact]
    public void WebhookInfo_reads_snowflake_strings()
    {
        const string json = """
            {
              "name": "test webhook",
              "type": 1,
              "channel_id": "199737254929760256",
              "guild_id": "199737254929760256",
              "id": "223704706495545344"
            }
            """;

        var webhook = JsonSerializer.Deserialize<WebhookInfo>(json, DiscordJson.Options);
        Assert.NotNull(webhook);
        Assert.Equal("199737254929760256", webhook.ChannelId);
        Assert.Equal("199737254929760256", webhook.GuildId);
        Assert.True(ulong.TryParse(webhook.ChannelId, out _));
    }

    [Fact]
    public void OAuth_token_treats_expires_in_as_seconds_and_binds_scope()
    {
        const string json = """
            {
              "access_token": "6qrZcUqja7812RVdnEKjpzOL4CvHBFG",
              "token_type": "Bearer",
              "expires_in": 604800,
              "refresh_token": "D43f5y0ahjqew82jZ4NViEr2YafMKhue",
              "scope": "identify guilds email"
            }
            """;

        var token = JsonSerializer.Deserialize<OAuth2ExchangeResponse>(json, DiscordJson.Options);
        Assert.NotNull(token);
        Assert.Equal(604800, token.ExpiresIn);
        Assert.Equal("identify guilds email", token.Scope);

        var expireAt = DateTimeOffset.UnixEpoch.AddSeconds(token.ExpiresIn);
        Assert.Equal(TimeSpan.FromDays(7), expireAt - DateTimeOffset.UnixEpoch);
    }

    [Fact]
    public void User_reads_global_name_and_string_id()
    {
        const string json = """
            {
              "id": "80351110224678912",
              "username": "Nelly",
              "discriminator": "0",
              "global_name": "Nelly",
              "avatar": "8342729096ea3675442027381ff50dfe",
              "email": "nelly@example.com"
            }
            """;

        var user = JsonSerializer.Deserialize<DiscordSelfUser>(json, DiscordJson.Options);
        Assert.NotNull(user);
        Assert.Equal(80351110224678912UL, user.Id);
        Assert.Equal("Nelly", user.GlobalName);
    }

    [Fact]
    public void Execute_webhook_json_uses_avatar_url_and_nested_embeds()
    {
        var payload = new DiscordMessageCreatePayload
        {
            Content = "hello",
            Username = "Hookio",
            AvatarUrl = "https://example.com/avatar.png",
            Embeds =
            [
                new DiscordEmbedPayload
                {
                    Title = "Title",
                    Description = "Body",
                    Url = "https://example.com",
                    Color = 16711680,
                    Author = new DiscordEmbedAuthor { Name = "Author", Url = "https://example.com/a", IconUrl = "https://example.com/i.png" },
                    Footer = new DiscordEmbedFooter { Text = "Footer", IconUrl = "https://example.com/f.png" },
                    Image = new DiscordEmbedMedia { Url = "https://example.com/image.png" },
                    Thumbnail = new DiscordEmbedMedia { Url = "https://example.com/thumb.png" },
                    Fields = [new DiscordEmbedField { Name = "Field", Value = "Value", Inline = true }]
                }
            ]
        };

        var json = JsonSerializer.Serialize(payload, DiscordJson.Options);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.Equal("https://example.com/avatar.png", root.GetProperty("avatar_url").GetString());
        Assert.False(root.TryGetProperty("avatar", out _));
        var embed = root.GetProperty("embeds")[0];
        Assert.Equal(JsonValueKind.Object, embed.ValueKind);
        Assert.Equal(16711680, embed.GetProperty("color").GetInt32());
        Assert.Equal("Author", embed.GetProperty("author").GetProperty("name").GetString());
        Assert.Equal("https://example.com/i.png", embed.GetProperty("author").GetProperty("icon_url").GetString());
        Assert.Equal("https://example.com/image.png", embed.GetProperty("image").GetProperty("url").GetString());
    }
}
