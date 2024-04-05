using Discord.Rest;
using Hookio.Contracts;
using Hookio.Database.Entities;
using Hookio.Enunms;

namespace Hookio.Database.Interfaces
{
    public interface IDataManager
    {
        #region users
        public Task<IEnumerable<RestUserGuild>> GetUserServers(DiscordRestClient client);
        public Task<CurrentUserResponse?> GetUser(ulong userId);
        public Task<User?> CreateUser(DiscordRestClient client, OAuth2ExchangeResponse token);
        public Task<string?> GetAccessToken(ulong userId);
        #endregion

        #region announcements
        public Task<SubscriptionResponse?> GetSubscription(ulong guildId, int id);
        public Task<SubscriptionResponse?> CreateSubscription(ulong guildId, SubscriptionRequest request);
        public Task<SubscriptionResponse?> UpdateSubscription(ulong guildId, int id, SubscriptionRequest request);
        public Task<List<SubscriptionResponse>?> GetSubscriptions(ulong guildId, SubscriptionType? provider);
        #endregion
    }
}
