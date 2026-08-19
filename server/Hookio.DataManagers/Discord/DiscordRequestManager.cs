using Hookio.Database;
using Hookio.DataManagers.Utils.Interfaces;
using Hookio.Discord.Contracts;
using Hookio.Discord.Interfaces;
using Hookio.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace Hookio.Discord
{
    public class DiscordRequestManager(ILogger<DiscordRequestManager> logger, ITaskQueue _queue, IDbContextFactory<HookioContext> contextFactory) : IDiscordRequestManager
    {
        public async Task<OAuth2ExchangeResponse?> ExchangeOAuth2Code(string code)
        {
            var discordResponse = await _queue.Enqueue(0, (_httpClient) => _httpClient.PostAsync("/api/v10/oauth2/token",
                new FormUrlEncodedContent(new Dictionary<string, string?>()
                {
                    { "code", code },
                    { "redirect_uri", Environment.GetEnvironmentVariable(EnvNames.DiscordRedirectUri) },
                    { "grant_type", "authorization_code" },
                    { "client_id", Environment.GetEnvironmentVariable(EnvNames.DiscordClientId)! },
                    { "client_secret", Environment.GetEnvironmentVariable(EnvNames.DiscordClientSecret)! },
                }
                )));
            if (!discordResponse.IsSuccessStatusCode) return null;
            var result = await discordResponse.Content.ReadFromJsonAsync<OAuth2ExchangeResponse>(DiscordJson.Options);
            if (result == null)
            {
                logger.LogInformation("[{FunctionName}]: Failed to code exchange", nameof(ExchangeOAuth2Code));
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
                var clientId = Environment.GetEnvironmentVariable(EnvNames.DiscordClientId)!;
                var clientSecret = Environment.GetEnvironmentVariable(EnvNames.DiscordClientSecret)!;
                var form = new FormUrlEncodedContent(new Dictionary<string, string?>()
                {
                    { "grant_type", "refresh_token" },
                    { "client_id", clientId },
                    { "client_secret", clientSecret },
                    { "refresh_token", user!.RefreshToken }
                });

                return await _httpClient.PostAsync("/api/v10/oauth2/token", form);
            });

            if (!discordResponse.IsSuccessStatusCode) return null;
            return await discordResponse.Content.ReadFromJsonAsync<OAuth2ExchangeResponse>(DiscordJson.Options);
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

            if (!discordResponse.IsSuccessStatusCode) return null;
            return await discordResponse.Content.ReadFromJsonAsync<DiscordSelfUser>(DiscordJson.Options);
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

            if (!discordResponse.IsSuccessStatusCode) return null;
            return await discordResponse.Content.ReadFromJsonAsync<DiscordSelfUser>(DiscordJson.Options);
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

            if (!discordResponse.IsSuccessStatusCode) return null;
            return await discordResponse.Content.ReadFromJsonAsync<IEnumerable<DiscordPartialGuild>>(DiscordJson.Options);
        }

        public async Task<DiscordWebhookResult> SendWebhookMessage(DiscordMessageCreatePayload payload, string webhookUrl)
        {
            try
            {
                var response = await _queue.Enqueue(2, (_httpClient) => _httpClient.PostAsJsonAsync($"{webhookUrl}?wait=true", payload, DiscordJson.Options));
                return await ReadWebhookResult(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send webhook message");
                return new DiscordWebhookResult(0, null);
            }
        }

        public async Task<DiscordWebhookResult> UpdateWebhookMessage(DiscordMessageCreatePayload payload, ulong messageId, string webhookUrl)
        {
            try
            {
                var response = await _queue.Enqueue(2, (_httpClient) => _httpClient.PatchAsJsonAsync($"{webhookUrl}/messages/{messageId}", payload, DiscordJson.Options));
                return await ReadWebhookResult(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to update webhook message");
                return new DiscordWebhookResult(0, null);
            }
        }

        private static async Task<DiscordWebhookResult> ReadWebhookResult(HttpResponseMessage response)
        {
            var status = (int)response.StatusCode;
            if (!response.IsSuccessStatusCode)
                return new DiscordWebhookResult(status, null);
            var message = await response.Content.ReadFromJsonAsync<DiscordPartialMessage>(DiscordJson.Options);
            return new DiscordWebhookResult(status, message);
        }
    }
}
