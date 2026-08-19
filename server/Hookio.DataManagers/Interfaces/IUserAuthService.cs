using Hookio.Contracts;
using Hookio.Database.Entities;
using Hookio.Discord.Contracts;
using Microsoft.AspNetCore.Http;

namespace Hookio.Database.Interfaces;

public interface IUserAuthService
{
    Task<CurrentUserResponse?> GetUser(ulong userId);
    Task<User> CreateUser(DiscordSelfUser user, OAuth2ExchangeResponse token);
    Task RevalidateUserAccessToken(ulong userId);
    Task<CurrentUserResponse?> Authenticate(HttpContext httpContext, string code);
}
