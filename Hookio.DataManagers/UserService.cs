using Discord.Rest;
using Hookio.Contracts.Discord;
using Hookio.Data;
using Hookio.Data.Entities;
using Hookio.Shared.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using Discord;
using Hookio.DataManagers.Interfaces;

namespace Hookio.DataManagers
{
    public class UserService(
        DiscordRestClient restClient,
        IHttpClientFactory httpClientFactory,
        IOptions<OAuth2> oauth2Options,
        IDbContextFactory<HookioContext> contextFactory
        ) : IUserService
    {
        private readonly DiscordRestClient _discordClient = restClient;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly OAuth2 _oauth2Options = oauth2Options.Value;
        private readonly IDbContextFactory<HookioContext> _contextFactory = contextFactory;

        public async Task<RestSelfUser?> Authenticate(string code, CancellationToken cancellationToken)
        {
            using HttpClient httpClient = _httpClientFactory.CreateClient("OAuth2");
            List<KeyValuePair<string, string>> formData = new()
            {
                { new("grant_type", "authorization_code") },
                { new("code", code) },
                { new("redirect_uri", _oauth2Options.RedirectURI) },
                { new("client_id", _oauth2Options.ClientId) },
                { new("client_secret", _oauth2Options.ClientSecret) },
                { new("scopes", _oauth2Options.Scopes) },
            };
            FormUrlEncodedContent content = new(formData);
            HttpResponseMessage result = await httpClient.PostAsync("/api/v10/oauth2/token", content, cancellationToken);
            OAuth2Response? response = await result.Content.ReadFromJsonAsync<OAuth2Response>(cancellationToken);
            if (response == null) return null;
            return await SaveUser(response, cancellationToken);
        }

        public async Task<List<RestUserGuild>> GetUserGuilds(ulong userId, CancellationToken cancellationToken)
        {
            using var ctx = await _contextFactory.CreateDbContextAsync(cancellationToken);
            var user = await ctx.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
            if (user == null) return [];
            if (user.ExpireAt < DateTimeOffset.UtcNow)
            {
                await RefreshToken(user, cancellationToken);
                user = await ctx.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
            }
            await _discordClient.LoginAsync(TokenType.Bearer, user!.AccessToken);
            var guilds = await _discordClient.GetGuildSummariesAsync(new() { CancelToken = cancellationToken }).FlattenAsync();
            await _discordClient.LogoutAsync();
            if (guilds == null) return [];
            return guilds.Where(x => x.IsOwner || x.Permissions.Has(GuildPermission.Administrator) || x.Permissions.Has(GuildPermission.ManageGuild)).ToList();
        }

        private async Task<RestSelfUser> SaveUser(OAuth2Response response, CancellationToken cancellationToken)
        {
            await _discordClient.LoginAsync(TokenType.Bearer, response.AccessToken);
            RestSelfUser currentUser = await _discordClient.GetCurrentUserAsync(new() { CancelToken = cancellationToken });

            using var ctx = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var existingUser = await ctx.Users.FirstOrDefaultAsync(x => x.Id == currentUser.Id, cancellationToken);
            if (existingUser != null)
            {
                existingUser.AccessToken = response.AccessToken;
                existingUser.RefreshToken = response.RefreshToken;
                existingUser.ExpireAt = DateTimeOffset.UtcNow.AddSeconds(response.ExpiresIn);
            }
            else
            {
                await ctx.Users.AddAsync(new()
                {
                    AccessToken = response.AccessToken,
                    RefreshToken = response.RefreshToken,
                    ExpireAt = DateTimeOffset.UtcNow.AddSeconds(response.ExpiresIn),
                    Id = currentUser.Id
                }, cancellationToken);
            }

            await ctx.SaveChangesAsync(cancellationToken);
            await _discordClient.LogoutAsync();
            return currentUser;
        }

        private async Task<RestSelfUser?> RefreshToken(User user, CancellationToken cancellationToken)
        {
            using HttpClient httpClient = _httpClientFactory.CreateClient("OAuth2");
            List<KeyValuePair<string, string>> formData = new()
            {
                { new("grant_type", "refresh_token") },
                { new("refresh_token", user.RefreshToken) },
                { new("client_id", _oauth2Options.ClientId) },
                { new("client_secret", _oauth2Options.ClientSecret) },
            };
            FormUrlEncodedContent content = new(formData);
            HttpResponseMessage result = await httpClient.PostAsync("/api/v10/oauth2/token", content, cancellationToken);
            OAuth2Response? response = await result.Content.ReadFromJsonAsync<OAuth2Response>(cancellationToken);
            if (response == null) return null;
            return await SaveUser(response, cancellationToken);
        }
    }
}
