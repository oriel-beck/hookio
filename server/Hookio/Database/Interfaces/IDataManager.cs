using Discord.Rest;
using Hookio.Contracts;
using Hookio.Database.Entities;

namespace Hookio.Database.Interfaces
{
    public interface IDataManager
    {
        #region users
        public Task<IEnumerable<RestUserGuild>> GetUserServers(DiscordRestClient client);
        public Task<CurrentUserResponse?> GetUser(ulong userId);
        public Task<User?> CreateUser(DiscordRestClient client, OAuth2ExchangeResponse token);
        public Task<string?> GetAccessToken(ulong userId);
        public Task<bool> CanUserAccessGuild(ulong userId, ulong guildId);
        #endregion

        #region announcements
        public Task<AnnouncementResponse?> GetAnnouncementById(int id);
        public Task<AnnouncementResponse?> CreateAnnouncement(ulong guildId, AnnouncementRequest request);
        // TODO: make a system that makes sure the user interacting with the announcement has perms in the provided guild
        //public Task<AnnouncementResponse?> UpdateAnnouncement(AnnouncementResponse response);
        #endregion
    }
}
