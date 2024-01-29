using Hookio.Contracts;
using Hookio.Database.Entities;

namespace Hookio.Database.Interfaces
{
    public interface IDataManager
    {
        #region users
        public Task<List<DiscordGuildResponse>> GetUserServers(string userId);
        public Task<CurrentUserResponse?> GetUser(string userId);
        public Task<User?> CreateUser(DiscordCurrentUserResponse user, DiscordTokenResponse token);
        public Task<string?> GetAccessToken(string userId);
        public Task<bool> CanUserAccessGuild(string userId, string guildId);
        #endregion

        #region announcements
        public Task<AnnouncementResponse?> GetAnnouncementById(int id);
        public Task<AnnouncementResponse?> CreateAnnouncement(string guildId, AnnouncementRequest request);
        // TODO: make a system that makes sure the user interacting with the announcement has perms in the provided guild
        //public Task<AnnouncementResponse?> UpdateAnnouncement(AnnouncementResponse response);
        #endregion
    }
}
