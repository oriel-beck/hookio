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
        public Task<SubscriptionResponse?> GetSubscriptionById(int id);
        public Task<SubscriptionResponse?> CreateSubscription(ulong guildId, SubscriptionRequest request);

        public Task<List<SubscriptionResponse>?> GetSubscriptions(ulong guildId, SubscriptionType? provider);
        // TODO: make a system that makes sure the user interacting with the announcement has perms in the provided guild
        //public Task<AnnouncementResponse?> UpdateAnnouncement(AnnouncementResponse response);
        #endregion
    }
}
