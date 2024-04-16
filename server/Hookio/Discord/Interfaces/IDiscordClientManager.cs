using Discord;
using Discord.Rest;
using Discord.Webhook;

namespace Hookio.Discord.Interfaces
{
    public interface IDiscordClientManager
    {
        public Task<DiscordRestClient> GetBearerClientAsync(string token);
        public DiscordWebhookClient GetWebhookClient(string webhookUrl);
        public void SendWebhookAsync(Database.Entities.Message message, string webhookUrl, string videoId);
        public void EditWebhookAsync(Database.Entities.Message message, ulong messageId, string webhookUrl);
    }
}
