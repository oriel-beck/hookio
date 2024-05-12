using Google.Apis.YouTube.v3.Data;
using Hookio.Contracts;
using Hookio.Database.Entities;
using Hookio.Discord.Contracts;
using Hookio.Enunms;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Hookio.Database.Interfaces
{
    public interface IDataManager
    {
        #region users
        public Task<IEnumerable<DiscordPartialGuild>?> GetUserGuilds(User user);
        public Task<CurrentUserResponse?> GetUser(ulong userId);
        public Task<User> CreateUser(DiscordSelfUser user, OAuth2ExchangeResponse token);
        public Task RevalidateUserAccessToken(ulong userId);
        public Task<CurrentUserResponse?> Authenticate(HttpContext httpContext, string code);
        public void CreateTokenAndSetCookie(HttpContext context, DiscordSelfUser user, IEnumerable<string>? guildIds);
        public void CreateTokenAndSetCookie(TokenValidatedContext context, DiscordSelfUser user, IEnumerable<string>? guildIds);
        public Task RefreshUserAuthentication(TokenValidatedContext context, SecurityToken token);
        #endregion

        #region subscriptions
        public Task<SubscriptionResponse?> GetSubscription(ulong guildId, int id);
        public Task<Entities.Subscription?> GetSubscription(int id);
        public Task<SubscriptionResponse?> CreateSubscription(ulong guildId, SubscriptionRequest request);
        public Task<SubscriptionResponse?> UpdateSubscription(ulong guildId, int id, SubscriptionRequest request);
        public Task<GuildSubscriptionsResponse> GetSubscriptions(ulong guildId, SubscriptionType? provider, bool withCounts = false);
        public Task<List<Entities.Subscription>> GetSubscriptions(Video video, EventType eventType);
        #endregion
    }
}
