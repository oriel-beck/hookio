using Discord.Rest;
using Hookio.Data.Entities;

namespace Hookio.DataManagers.Interfaces
{
    public interface IUserService
    {
        Task<RestSelfUser?> Authenticate(string code, CancellationToken cancellationToken);
        Task<List<RestUserGuild>> GetUserGuilds(User user, CancellationToken cancellationToken);

        Task<User?> GetUser(ulong id, CancellationToken cancellationToken);
    }
}