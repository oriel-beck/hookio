using Hookio.Contracts;
using Hookio.Database.Entities;
using Hookio.Discord.Contracts;
using Hookio.Enums;

namespace Hookio.Database;

internal static class SubscriptionMapper
{
    public static CurrentUserResponse ToCurrentUser(DiscordSelfUser discordUser, IEnumerable<GuildResponse> guilds) => new()
    {
        Discriminator = discordUser.Discriminator,
        Id = discordUser.Id.ToString(),
        Username = discordUser.GlobalName is null ? discordUser.Username : discordUser.GlobalName,
        Premium = 0,
        Avatar = discordUser.GetAvatarUrl(),
        Guilds = guilds
    };

    public static IEnumerable<GuildResponse> ToGuilds(IEnumerable<DiscordPartialGuild> guilds) =>
        guilds.Where(guild => (guild.Permissions & 0x0000000000000020) != 0).Select(guild => new GuildResponse
        {
            Id = guild.Id.ToString(),
            Name = guild.Name!,
            Icon = guild.IconUrl,
        });

    public static SubscriptionResponse ToContract(Subscription subscription) => new()
    {
        Id = subscription.Id,
        SubscriptionType = subscription.SubscriptionType,
        GuildId = subscription.GuildId,
        Events = (subscription.Events ?? []).Select(ToContract).ToDictionary(ev => ev.EventType),
        ChannelId = subscription.WebhookChannel,
        Url = ToPublicUrl(subscription),
        Disabled = subscription.Disabled
    };

    private static MessageResponse ToContract(Message message) => new()
    {
        Id = message.Id,
        Content = message.Content,
        Embeds = (message.Embeds ?? []).Select(ToContract).ToList(),
        Avatar = message.WebhookAvatar,
        Username = message.WebhookUsername,
    };

    private static EmbedResponse ToContract(Embed embed) => new()
    {
        Id = embed.Id,
        Index = embed.Index,
        Description = embed.Description,
        TitleUrl = embed.TitleUrl,
        Title = embed.Title,
        Color = embed.Color,
        Image = embed.Image,
        Author = embed.Author,
        AuthorUrl = embed.AuthorUrl,
        AuthorIcon = embed.AuthorIcon,
        Footer = embed.Footer,
        FooterIcon = embed.FooterIcon,
        Thumbnail = embed.Thumbnail,
        AddTimestamp = embed.AddTimestamp,
        Fields = (embed.Fields ?? []).Select(ToContract).ToList()
    };

    private static EmbedFieldResponse ToContract(EmbedField field) => new()
    {
        Id = field.Id,
        Name = field.Name,
        Value = field.Value,
        Inline = field.Inline,
        Index = field.Index,
    };

    private static EventResponse ToContract(Event eventEntity) => new()
    {
        Id = eventEntity.Id,
        EventType = eventEntity.Type,
        Message = ToContract(eventEntity.Message),
    };

    internal static string? ToPublicUrl(Subscription subscription)
    {
        if (subscription.SubscriptionType == SubscriptionType.Twitch && !string.IsNullOrEmpty(subscription.TwitchLogin))
            return $"https://www.twitch.tv/{subscription.TwitchLogin}";

        var feedUrl = subscription.Feed?.Url;
        if (subscription.SubscriptionType == SubscriptionType.Youtube && feedUrl is not null)
        {
            const string marker = "channel_id=";
            var idx = feedUrl.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
            {
                var id = feedUrl[(idx + marker.Length)..];
                var amp = id.IndexOf('&');
                if (amp >= 0) id = id[..amp];
                return $"https://www.youtube.com/channel/{id}";
            }
        }
        return feedUrl;
    }

    internal static int GetEmbedLength(Embed embed)
    {
        int num = embed.Title?.Length ?? 0;
        int valueOrDefault = (embed.Author?.Length).GetValueOrDefault();
        int num2 = embed.Description?.Length ?? 0;
        int valueOrDefault2 = (embed.Footer?.Length).GetValueOrDefault();
        int fields = (embed.Fields ?? []).Sum(f => (f.Name?.Length ?? 0) + (f.Value?.Length ?? 0));
        return num + valueOrDefault + num2 + valueOrDefault2 + fields;
    }
}
