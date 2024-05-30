using Hookio.Discord.Contracts;

namespace Hookio.Discord.Interfaces
{
    public interface IDiscordRequestManager
    {
        Task<OAuth2ExchangeResponse?> ExchangeOAuth2Code(string code);
        Task<DiscordSelfUser?> GetDiscordUser(ulong userId);
        Task<DiscordSelfUser?> GetDiscordUser(string accessToken);
        Task<IEnumerable<DiscordPartialGuild>?> GetDiscordUserGuilds(string accessToken);
        Task<OAuth2ExchangeResponse?> RefreshOAuth2(ulong userId);
        Task<DiscordPartialMessage?> SendWebhookMessage(DiscordMessageCreatePayload payload, string webhookUrl);
        Task<DiscordPartialMessage?> UpdateWebhookMessage(DiscordMessageCreatePayload payload, ulong messageId, string webhookUrl);
    }
}