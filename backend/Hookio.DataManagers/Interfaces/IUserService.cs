using Discord.Rest;
using Hookio.Contracts.Discord;
using Microsoft.AspNetCore.Http;

namespace Hookio.DataManagers.Interfaces
{
    public interface IUserService
    {
        Task<OAuth2Response?> Authenticate(string code, CancellationToken cancellationToken);
        Task<RestSelfUser?> GetRestUser(string accessToken, CancellationToken cancellationToken);
        //Task<User?> GetUser(ulong id, CancellationToken cancellationToken);
        Task<List<RestUserGuild>> GetUserGuilds(string accessToken, CancellationToken cancellationToken);
        Task<OAuth2Response?> RefreshToken(string refreshToken, CancellationToken cancellationToken);
        Task ValidateSessionData(ISession session, CancellationToken cancellationToken);
    }
}