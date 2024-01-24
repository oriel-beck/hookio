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
        #endregion
    }
}
