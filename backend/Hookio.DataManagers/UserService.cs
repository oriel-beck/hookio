using Discord.Rest;
using Hookio.Contracts.Discord;
using Hookio.Data;
using Hookio.Shared.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using Discord;
using Hookio.DataManagers.Interfaces;
using Microsoft.AspNetCore.Http;
using Hookio.Shared.Extensions;

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

        public async Task<OAuth2Response?> Authenticate(string code, CancellationToken cancellationToken)
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
            return response;
        }

        public async Task<List<RestUserGuild>> GetUserGuilds(string accessToken, CancellationToken cancellationToken)
        {
            using var ctx = await _contextFactory.CreateDbContextAsync(cancellationToken);

            await _discordClient.LoginAsync(TokenType.Bearer, accessToken);
            var guilds = await _discordClient.GetGuildSummariesAsync(new() { CancelToken = cancellationToken }).FlattenAsync();
            await _discordClient.LogoutAsync();

            if (guilds == null) return [];
            return guilds.Where(x => x.IsOwner || x.Permissions.Has(GuildPermission.Administrator) || x.Permissions.Has(GuildPermission.ManageGuild)).ToList();
        }

        public async Task<RestSelfUser?> GetRestUser(string accessToken, CancellationToken cancellationToken)
        {
            await _discordClient.LoginAsync(TokenType.Bearer, accessToken);
            var discordUser = await _discordClient.GetCurrentUserAsync(new() { CancelToken = cancellationToken });
            await _discordClient.LogoutAsync();

            return discordUser;
        }

        public async Task<OAuth2Response?> RefreshToken(string refreshToken, CancellationToken cancellationToken)
        {
            using HttpClient httpClient = _httpClientFactory.CreateClient("OAuth2");
            List<KeyValuePair<string, string>> formData = new()
            {
                { new("grant_type", "refresh_token") },
                { new("refresh_token", refreshToken) },
                { new("client_id", _oauth2Options.ClientId) },
                { new("client_secret", _oauth2Options.ClientSecret) },
            };
            FormUrlEncodedContent content = new(formData);
            HttpResponseMessage result = await httpClient.PostAsync("/api/v10/oauth2/token", content, cancellationToken);
            OAuth2Response? response = await result.Content.ReadFromJsonAsync<OAuth2Response>(cancellationToken);
            return response;
        }

        public async Task ValidateSessionData(ISession session, CancellationToken cancellationToken)
        {
            // validate if access token cache is valid
            var accessToken = session.GetWithExpiry<string>("accessToken");
            if (accessToken == null)
            {
                var refreshToken = session.GetWithExpiry<string>("refreshToken");
                if (refreshToken == null) return;
                var response = await RefreshToken(refreshToken!, cancellationToken);
                accessToken = response!.AccessToken;
                session.SetWithExpiry("accessToken", accessToken, TimeSpan.FromSeconds(response.ExpiresIn));
                session.SetWithExpiry("refreshToken", response!.RefreshToken, null);
            }

            // TODO: Discord.Net.Rest classes cannot be used to deserialize, only serialize. Use a custom class to take required attributes

            // validate if user cache is valid
            var user = session.GetWithExpiry<RestSelfUser>("user");
            if (user == null)
            {
                user = await GetRestUser(accessToken, cancellationToken);
                session.SetWithExpiry("user", user, TimeSpan.FromHours(1));
            }

            // validate if guilds cache is valid
            var guilds = session.GetWithExpiry<List<RestUserGuild>>("guilds");
            if (guilds == null)
            {
                guilds = await GetUserGuilds(accessToken, cancellationToken);
                session.SetWithExpiry("guilds", guilds, TimeSpan.FromMinutes(5));
            }
        }
    }
}
