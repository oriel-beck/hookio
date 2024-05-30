using Hookio.Database;
using Hookio.DataManagers.Utils.Interfaces;
using Hookio.Discord.Contracts;
using Hookio.Discord.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Net.Http.Json;

namespace Hookio.Discord
{
    public class DiscordRequestManager(ILogger<DiscordRequestManager> logger, ITaskQueue _queue, IConnectionMultiplexer redis, IDbContextFactory<HookioContext> contextFactory) : IDiscordRequestManager
    {
        private readonly IDatabase _redisDatabase = redis.GetDatabase();

        public async Task<OAuth2ExchangeResponse?> ExchangeOAuth2Code(string code)
        {

            var discordResponse = await _queue.Enqueue(0, (_httpClient) => _httpClient.PostAsync($"/api/v10/oauth2/token",
                new FormUrlEncodedContent(new Dictionary<string, string?>()
                {
                    { "code", code },
                    { "redirect_uri", Environment.GetEnvironmentVariable("DISCORD_REDIRECT_URI") },
                    { "grant_type", "authorization_code" },
                    { "client_id", Environment.GetEnvironmentVariable("DISCORD_CLIENT_ID")! },
                    { "client_secret", Environment.GetEnvironmentVariable("DISCORD_CLIENT_SECRET")! },
                    { "scopes", "identify guilds email" }
                }
                )));
            var result = await discordResponse.Content.ReadFromJsonAsync<OAuth2ExchangeResponse>();
            if (result == null)
            {
                logger.LogInformation("[{FunctionName}]: Failed to code exchange with code '{Scopes}'", nameof(ExchangeOAuth2Code), code);
                return null;
            }


            if (result.Scope == null || !result.Scope.Contains("email") || !result.Scope.Contains("identify") || !result.Scope.Contains("guilds"))
            {
                logger.LogInformation("[{FunctionName}]: Did not get all required scopes, cancelled login, got scopes '{Scopes}'", nameof(ExchangeOAuth2Code), result.Scope);
                return null;
            }

            return result;
        }

        public async Task<OAuth2ExchangeResponse?> RefreshOAuth2(ulong userId)
        {
            var discordResponse = await _queue.Enqueue(0, async (_httpClient) =>
            {
                var ctx = await contextFactory.CreateDbContextAsync();
                var user = await ctx.Users.Where(user => user.Id == userId).FirstOrDefaultAsync();
                var clientId = Environment.GetEnvironmentVariable("DISCORD_CLIENT_ID")!;
                var clientSecret = Environment.GetEnvironmentVariable("DISCORD_CLIENT_SECRET")!;
                var form = new FormUrlEncodedContent(new Dictionary<string, string?>()
                {
                    { "grant_type", "refresh_token" },
                    { "client_id", clientId },
                    { "client_secret", clientSecret },
                    { "refresh_token", user!.RefreshToken }
                });

                return await _httpClient.PostAsync($"/api/v10/oauth2/token", form);
            });

            var result = await discordResponse.Content.ReadFromJsonAsync<OAuth2ExchangeResponse>();
            return result;
        }

        public async Task<DiscordSelfUser?> GetDiscordUser(ulong userId)
        {
            var discordResponse = await _queue.Enqueue(1, async (_httpClient) =>
            {
                var httpRequestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Get
                };
                var ctx = await contextFactory.CreateDbContextAsync();
                var user = await ctx.Users.Where(u => u.Id == userId).FirstOrDefaultAsync();
                httpRequestMessage.Headers.Add("Authorization", $"Bearer {user!.AccessToken}");
                httpRequestMessage.RequestUri = new Uri("https://discord.com/api/v10/users/@me");
                return await _httpClient.SendAsync(httpRequestMessage);
            });

            var res = await discordResponse.Content.ReadFromJsonAsync<DiscordSelfUser>();
            return res;
        }

        public async Task<DiscordSelfUser?> GetDiscordUser(string accessToken)
        {
            var discordResponse = await _queue.Enqueue(1, (_httpClient) =>
            {
                var httpRequestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Get
                };
                httpRequestMessage.Headers.Add("Authorization", $"Bearer {accessToken}");
                httpRequestMessage.RequestUri = new Uri("https://discord.com/api/v10/users/@me");
                return _httpClient.SendAsync(httpRequestMessage);
            });

            var res = await discordResponse.Content.ReadFromJsonAsync<DiscordSelfUser>();
            return res;
        }

        public async Task<IEnumerable<DiscordPartialGuild>?> GetDiscordUserGuilds(string accessToken)
        {
            var discordResponse = await _queue.Enqueue(1, (_httpClient) =>
            {
                var httpRequestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Get
                };
                httpRequestMessage.Headers.Add("Authorization", $"Bearer {accessToken}");
                httpRequestMessage.RequestUri = new Uri("https://discord.com/api/v10/users/@me/guilds");
                return _httpClient.SendAsync(httpRequestMessage);
            });

            var res = await discordResponse.Content.ReadFromJsonAsync<IEnumerable<DiscordPartialGuild>>();
            return res;
        }

        public async Task<DiscordPartialMessage?> SendWebhookMessage(DiscordMessageCreatePayload payload, string webhookUrl)
        {
            try
            {
                var response = await _queue.Enqueue(2, (_httpClient) => _httpClient.PostAsJsonAsync($"{webhookUrl}?wait=true", payload));
                return await response.Content.ReadFromJsonAsync<DiscordPartialMessage>();
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to send embed, excpetion: {Ex}", ex.Message);
                return null;
            }
        }

        public async Task<DiscordPartialMessage?> UpdateWebhookMessage(DiscordMessageCreatePayload payload, ulong messageId, string webhookUrl)
        {
            try
            {
                var response = await _queue.Enqueue(2, (_httpClient) => _httpClient.PatchAsJsonAsync($"{webhookUrl}/messages/{messageId}", payload));
                return await response.Content.ReadFromJsonAsync<DiscordPartialMessage>();
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to update embed, excpetion: {Ex}", ex.Message);
                return null;
            }
        }

    }
}
