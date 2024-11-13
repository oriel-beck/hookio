using Discord.Rest;

namespace Hookio.DataManagers.Interfaces
{
    public interface IUserService
    {
        Task<RestSelfUser?> Authenticate(string code, CancellationToken cancellationToken);
        Task<List<RestUserGuild>> GetUserGuilds(ulong userId, CancellationToken cancellationToken);
    }
}