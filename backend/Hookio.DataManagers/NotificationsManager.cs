using Discord;
using Discord.Webhook;
using Google.Apis.YouTube.v3.Data;
using Hookio.Contracts.YouTube;
using Hookio.Data.Entities;
using Hookio.DataManagers.Interfaces;
using Hookio.Shared.Enums;
using Hookio.Shared.Extensions;
using System.Collections.Concurrent;

namespace Hookio.DataManagers
{
    /// <summary>
    /// This class manages the notifications being sent to discord, it manages the webhooks and their ratelimits <br/>
    /// Ratelimit: 5/1s per channel, just in case
    /// </summary>
    /// <param name="videosCacheManager"></param>
    /// <param name="youTubeManager"></param>
    public partial class NotificationsManager(IVideosCacheManager videosCacheManager, IYouTubeManager youTubeManager) : INotificationsManager
    {
        private readonly IVideosCacheManager _videosCacheManager = videosCacheManager;
        private readonly IYouTubeManager _youTubeManager = youTubeManager;
        /// <summary>
        /// Manages the per channel ratelimit of 5/1s
        /// </summary>
        private readonly ConcurrentDictionary<string, System.Threading.Channels.Channel<Func<Task>>> _channelQueues = new();

        /// <summary>
        /// Gets or creates a new channel queue
        /// </summary>
        /// <param name="channelId"></param>
        /// <returns></returns>
        private System.Threading.Channels.Channel<Func<Task>> GetChannelQueue(string channelId)
        {
            return _channelQueues.GetOrAdd(channelId, _ =>
            {
                var channel = System.Threading.Channels.Channel.CreateUnbounded<Func<Task>>();
                Task.Run(async () => await ProcessQueue(channel));
                return channel;
            });
        }

        /// <summary>
        /// Proccesses the channel queue, sends/updates/deletes the messages with a delay of 200ms in each channel
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        private static async Task ProcessQueue(System.Threading.Channels.Channel<Func<Task>> channel)
        {
            var delay = TimeSpan.FromMilliseconds(200); // 5 requests per second = 200ms delay

            await foreach (var task in channel.Reader.ReadAllAsync())
            {
                try
                {
                    await task();
                }
                catch (Exception ex)
                {
                    // Log or handle exceptions
                    Console.WriteLine($"Error processing webhook task: {ex.Message}");
                }

                // Ensure rate limiting
                await Task.Delay(delay);
            }
        }

        /// <summary>
        /// Sends the new notifications to Discord (Using the queue) <br/>
        /// This sends, updates or deletes the message, depending on the message type
        /// </summary>
        /// <param name="ytvideo"></param>
        /// <param name="ytchannel"></param>
        /// <param name="ytfeed"></param>
        /// <param name="webhookUrl"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendNotification(Video ytvideo, Channel ytchannel, YouTubeFeed ytfeed, string webhookUrl, Message message)
        {
            var webhookClient = new DiscordWebhookClient(webhookUrl);
            if (webhookClient == null) return; // TODO: disable the subscription
            var templateData = _youTubeManager.GetTemplateStrings(ytvideo, ytchannel, ytfeed);
            if (templateData == null) return;
            ulong? originalMessage;
            (string?, IEnumerable<Embed>) messageData;

            // Add task to the channel queue
            var channelQueue = GetChannelQueue(message.Subscription!.ChannelId);
            switch (message.Action)
            {
                case MessageAction.CreateMessage:
                    // Generate and send the Discord message
                    messageData = GenerateDiscordMessage(message, templateData);
                    await channelQueue.Writer.WriteAsync(async () =>
                    {
                        var discordMessage = await webhookClient.SendMessageAsync(
                            text: messageData.Item1,
                            embeds: messageData.Item2,
                            avatarUrl: message.Subscription?.WebhookAvatar?.ToTemplateString(templateData),
                            username: message.Subscription?.WebhookUsername.ToTemplateString(templateData)
                        );

                        // Cache the video message
                        await _videosCacheManager.Set(ytvideo.Id, discordMessage);
                    });
                    break;

                case MessageAction.UpdateMessage:
                    // Handle message updates (if applicable)
                    originalMessage = await _videosCacheManager.Get(ytvideo.Id);
                    if (originalMessage == null) return;
                    messageData = GenerateDiscordMessage(message, templateData);
                    await channelQueue.Writer.WriteAsync(async () =>
                    {
                        await webhookClient.ModifyMessageAsync((ulong)originalMessage, (props) =>
                        {
                            props.Content = messageData.Item1;
                            props.Embeds = Optional.Create(messageData.Item2);
                        });
                    });
                    break;

                case MessageAction.DeleteMessage:
                    // Handle message deletions (if applicable)
                    originalMessage = await _videosCacheManager.Get(ytvideo.Id);
                    if (originalMessage == null) return;
                    await channelQueue.Writer.WriteAsync(async () =>
                    {
                        await webhookClient.DeleteMessageAsync((ulong)originalMessage);
                    });
                    break;
            }
        }

        private static (string?, IEnumerable<Embed>) GenerateDiscordMessage(Message message, Dictionary<string, string> templateData)
        {
            var embeds = new List<Embed>();

            foreach (var embed in message.Embeds)
            {
                var builder = new EmbedBuilder();

                foreach (var field in embed.Fields)
                {
                    builder.AddField(field.Name.ToTemplateString(templateData), field.Value.ToTemplateString(templateData), field.Inline);
                }

                if (embed.Title != null) builder.WithTitle(embed.Title.ToTemplateString(templateData));
                if (embed.Description != null) builder.WithDescription(embed.Description.ToTemplateString(templateData));
                if (embed.Url != null) builder.WithUrl(embed.Url);
                if (embed.Color != null) builder.WithColor((uint)embed.Color);
                if (embed.Footer != null && embed.Footer.Text != null) builder.WithFooter(embed.Footer.Text.ToTemplateString(templateData), embed.Footer.IconUrl);
                if (embed.Image != null) builder.WithImageUrl(embed.Image.Url);
                if (embed.Thumbnail != null) builder.WithThumbnailUrl(embed.Thumbnail.Url);
                if (embed.Author != null && embed.Author.Name != null) builder.WithAuthor(embed.Author.Name.ToTemplateString(templateData), embed.Author.IconUrl, embed.Author.Url);

                embeds.Add(builder.Build());
            }

            return (message.Content?.ToTemplateString(templateData), embeds);
        }
    }
}
