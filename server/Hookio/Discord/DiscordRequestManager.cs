using Discord;
using Hookio.Discord.Contracts;
using StackExchange.Redis;
using System.Globalization;

namespace Hookio.Discord
{
    public class DiscordRequestManager(ILogger<DiscordRequestManager> logger, TaskQueue _queue, ConnectionMultiplexer redis)
    {
        private readonly IDatabase _redisDatabase = redis.GetDatabase();
        private readonly HttpClient _httpClient = new()
        {
            BaseAddress = new Uri("https://discord.com")
        };

        public async Task<OAuth2ExchangeResponse?> ExchangeOAuth2Code(string code)
        {

            var discordResponse = await _queue.Enqueue(0, () => _httpClient.PostAsync($"/api/v10/oauth2/token",
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

            if (!result.Scope.Contains("email") || !result.Scope.Contains("identify") || !result.Scope.Contains("guilds"))
            {
                logger.LogInformation("[{FunctionName}]: Did not get all required scopes, cancelled login, got scopes '{Scopes}'", nameof(ExchangeOAuth2Code), result.Scope);
                return null;
            }

            return result;
        }

        public async Task<OAuth2ExchangeResponse?> RefreshOAuth2(string refreshToken)
        {

            var discordResponse = await _httpClient.PostAsync($"/api/v10/oauth2/token",
            new FormUrlEncodedContent(new Dictionary<string, string?>()
                {
                    { "grant_type", "refresh_token" },
                    { "client_id", Environment.GetEnvironmentVariable("DISCORD_CLIENT_ID")! },
                    { "client_secret", Environment.GetEnvironmentVariable("DISCORD_CLIENT_SECRET")! },
                    { "refresh_token", refreshToken }
                }
            ));
            var result = await discordResponse.Content.ReadFromJsonAsync<OAuth2ExchangeResponse>();
            return result;
        }

        public async Task<DiscordSelfUser?> GetDiscordUser(string accessToken)
        {
            var discordResponse = await _queue.Enqueue(1, () =>
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
            var discordResponse = await _queue.Enqueue(1, () =>
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

        public async Task<DiscordPartialMessage?> SendWebhookMessage(Database.Entities.Message message, string webhookUrl)
        {
            var embeds = ConvertEntityEmbedToDiscordEmbed(message.Embeds);
            try
            {
                var response = await _queue.Enqueue(3, () => _httpClient.PostAsJsonAsync($"{webhookUrl}?wait=true", embeds));
                return await response.Content.ReadFromJsonAsync<DiscordPartialMessage>();
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to send embed, excpetion: {Ex}", ex.Message);
                return null;
            }
        }

        public async Task<DiscordPartialMessage?> UpdateWebhookMessage(Database.Entities.Message message, ulong messageId, string webhookUrl)
        {
            var embeds = ConvertEntityEmbedToDiscordEmbed(message.Embeds);
            try
            {
                var response = await _queue.Enqueue(3, () => _httpClient.PatchAsJsonAsync(webhookUrl, embeds));
                return await response.Content.ReadFromJsonAsync<DiscordPartialMessage>();
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to update embed, excpetion: {Ex}", ex.Message);
                return null;
            }
        }
        private static IEnumerable<Embed> ConvertEntityEmbedToDiscordEmbed(List<Database.Entities.Embed> embeds)
        {
            return embeds.Select((embed) =>
            {
                var embedBuilder = new EmbedBuilder()
                    .WithTitle(embed.Title)
                    .WithUrl(embed.TitleUrl)
                    .WithDescription(embed.Description)
                    .WithImageUrl(embed.Image)
                    .WithThumbnailUrl(embed.Thumbnail)
                    .WithFooter(builder => builder.WithText(embed.Footer).WithIconUrl(embed.FooterIcon))
                    .WithAuthor(builder => builder.WithName(embed.Author).WithUrl(embed.AuthorUrl).WithIconUrl(embed.AuthorIcon))
                    .WithFields(ConvertEntityEmbedFieldToEmbedField(embed.Fields));
                if (embed.AddTimestamp) embedBuilder.WithCurrentTimestamp();
                if (embed.Color is not null)
                {
                    uint decValue = uint.Parse(embed.Color[1..], NumberStyles.HexNumber);
                    embedBuilder.Color = new Color(decValue);
                }
                return embedBuilder.Build();
            });
        }

        private static IEnumerable<EmbedFieldBuilder> ConvertEntityEmbedFieldToEmbedField(List<Database.Entities.EmbedField> embedFields)
        {
            return embedFields.Select((field) => new EmbedFieldBuilder().WithName(field.Name).WithValue(field.Value).WithIsInline(field.Inline));
        }
    }
}
