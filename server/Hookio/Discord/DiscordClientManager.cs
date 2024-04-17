using Discord.Rest;
using Discord.Webhook;
using Discord;
using Hookio.Discord.Interfaces;
using System.Globalization;
using StackExchange.Redis;

namespace Hookio.Discord
{
    public class DiscordClientManager(ILogger<DiscordClientManager> logger, IConnectionMultiplexer connectionMultiplexer) : IDiscordClientManager
    {
        private readonly IDatabase _redisDatabase = connectionMultiplexer.GetDatabase();
        private readonly RequestQueue requestQueue = new(30);
        // Set of `{YT_SENT_VIDEOS}-videoId` : dbMessageId-messageId
        const string YT_MSGS_SENT = "youtube_messages_sent";

        public async Task<DiscordRestClient> GetBearerClientAsync(string token)
        {
            var client = new DiscordRestClient();
            await client.LoginAsync(TokenType.Bearer, token);
            return client;
        }

        public DiscordWebhookClient GetWebhookClient(string webhookUrl)
        {
            var client = new DiscordWebhookClient(webhookUrl);
            return client;
        }

        public void SendWebhookAsync(Database.Entities.Message message, string webhookUrl, string videoId)
        {
            requestQueue.Enqueue(async () =>
            {
                var client = GetWebhookClient(webhookUrl);
                var embeds = ConvertEntityEmbedToDiscordEmbed(message.Embeds);
                try
                {
                    var resMessage = await client.SendMessageAsync(text: message.Content, embeds: embeds, username: message.WebhookUsername, avatarUrl: message.WebhookAvatar);
                    // Create a set for this videoId in case of editing
                    await _redisDatabase.SetAddAsync($"{YT_MSGS_SENT}-{videoId}", $"{message.Id}-{resMessage}");
                }
                catch (Exception ex)
                {
                    logger.LogError("Failed to send embed, excpetion: {Ex}", ex.Message);
                }
                finally
                {
                    client.Dispose();
                }
            });
        }

        public void EditWebhookAsync(Database.Entities.Message message, ulong messageId, string webhookUrl)
        {
            requestQueue.Enqueue(async () =>
            {
                var client = GetWebhookClient(webhookUrl);
                var embeds = ConvertEntityEmbedToDiscordEmbed(message.Embeds);
                try
                {
                    await client.ModifyMessageAsync(messageId, (m) =>
                    {
                        m.Content = message.Content;
                        m.Embeds = Optional.Create(embeds);
                    });
                }
                catch (Exception ex)
                {
                    logger.LogError("Failed to edit embed, excpetion: {Ex}", ex.Message);
                }
                finally
                {
                    client.Dispose();
                }
            });
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

    internal class RequestQueue(int maxRequestsPerSecond)
    {
        private readonly SemaphoreSlim semaphore = new(maxRequestsPerSecond);
        private readonly Queue<Func<Task>> queue = new();
        private readonly object lockObject = new();
        private bool isProcessingQueue;

        public void Enqueue(Func<Task> taskGenerator)
        {
            // Lock the queue for thread-safe access
            lock (lockObject)
            {
                queue.Enqueue(taskGenerator);
                if (!isProcessingQueue)
                {
                    isProcessingQueue = true;
                    Task.Run(ProcessQueue);
                }
            }
        }

        private async Task ProcessQueue()
        {
            while (true)
            {
                Func<Task>? taskGenerator = null;
                lock (lockObject)
                {
                    if (queue.Count > 0)
                    {
                        taskGenerator = queue.Dequeue();
                    }
                    else
                    {
                        isProcessingQueue = false;
                        break; // Exit the loop if the queue is empty
                    }
                }

                if (taskGenerator != null)
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        await taskGenerator();
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }
            }
        }
    }
}