using Google.Apis.YouTube.v3.Data;
using Hookio.Database.Interfaces;
using Hookio.Discord.Contracts;
using Hookio.Discord.Interfaces;
using Hookio.Enunms;
using Hookio.Utils.Contracts;
using Hookio.Utils.Interfaces;
using StackExchange.Redis;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Hookio.Utils
{
    /// <summary>
    /// This is how this works
    /// When a subscription is created, it is saved in a sorted set YT_SUBS_EXPIRED, with the score being TIMESTAMP + 10d,
    /// every hour, that sorted set is fetched based on the current timestamp, any subscriptions that have passed will resubscribe.
    /// 
    /// When a message is sent, the video ID is set in a sorted set YT_SENT_VIDEOS, with the score being TIMESTAMP + 12h
    /// every hour, that sorted set is fetched based on the current timestamp, any video that passed that will be fetched.
    /// 
    /// Alongside the video being saved, messages will be sent, then saved as `dbMessageId-messageId` in a set YT_MSGS_SENT-{videoId}, 
    /// when a video is sent, the message sent is saved alongside the ID of the db message, when a video is edited, the db message for the update event is fetched and the message is updated.
    /// </summary>
    public partial class YoutubeService : IYoutubeService
    {
        private readonly string? IDENTIFIER = Environment.GetEnvironmentVariable("YT_IDENTIFIER");
        const string YT_CALLBACK_URL = "https://hookio.gg/api/youtube/callback";
        // Set of channelId : score (timestamp)
        const string YT_SUBS_EXPIRED = "youtube_subscriptions_expiration";
        // Set of `{YT_SENT_VIDEOS}-videoId` : subscriptionId-DiscordMessageId
        const string YT_MSGS_SENT = "youtube_messages_sent";
        // Set of videoId : score (timestamp)
        const string YT_SENT_VIDEOS = "youtube_videos_sent";
        const int ONE_HOUR_MS = 3600000;
        private readonly HttpClient _httpClient = new();
        private readonly IDatabase _redisDatabase;
        private readonly IDiscordRequestManager _discordRequestManager;

        public YoutubeService(IConnectionMultiplexer connectionMultiplexer, IDiscordRequestManager discordRequestManager)
        {
            _redisDatabase = connectionMultiplexer.GetDatabase();
            _discordRequestManager = discordRequestManager;
            ResubTask();
            CleanupTask();
        }

        public async Task<bool?> Subscribe(string channelId, bool subscribe = true)
        {
            var content = new FormUrlEncodedContent(
            [
                new("hub.mode", subscribe ? "subscribe" : "unsubscribe"),
                new("hub.verify_token", IDENTIFIER!),
                new("hub.verify", "async"),
                new("hub.callback", YT_CALLBACK_URL),
                new("hub.topic", $"https://www.youtube.com/xml/feeds/videos.xml?channel_id={channelId}"),
            ]);

            var res = await _httpClient.PostAsync("https://pubsubhubbub.appspot.com/subscribe", content);
            var data = await res.Content.ReadAsStringAsync();
            Console.WriteLine(data);
            if (res.IsSuccessStatusCode) return null;
            return true;
        }
        public async void PublishVideo(Video video, Channel channel, IDataManager _dataManager)
        {
            // get all subscriptions for this channel
            var subscriptions = await _dataManager.GetSubscriptions(video, EventType.YoutubeVideoUploaded);
            foreach (var subscription in subscriptions)
            {
                // first event will always be the correct one due to the restriction of EventType above
                var subscriptionEvent = subscription.Events.FirstOrDefault();
                if (subscriptionEvent == null) continue;

                var templateHandler = TemplateHandler.InitiateYoutubeTemplateHandler(video, channel);

                var payload = new DiscordMessageCreatePayload
                {
                    Username = templateHandler.Parse(subscriptionEvent.Message.WebhookUsername),
                    Avatar = templateHandler.Parse(subscriptionEvent.Message.WebhookAvatar),
                    Content = templateHandler.Parse(subscriptionEvent.Message.Content),
                    embeds = DiscordUtils.ConvertEntityEmbedToDiscordEmbed(subscriptionEvent.Message.Embeds, templateHandler)
                };

                var res = await _discordRequestManager.SendWebhookMessage(payload, subscription.WebhookUrl);
                if (res == null) continue;

                // Create a set for this videoId in case of editing // TODO: res should be a class of WebhookResponse
                await _redisDatabase.SetAddAsync($"{YT_MSGS_SENT}-{video.Id}", $"{subscription.Id}-{res.Id}");
            }
            // add the video ID to the sorted set to be deleted in 12h
            await _redisDatabase.SortedSetAddAsync(YT_SENT_VIDEOS, video.Id, DateTimeOffset.UtcNow.AddHours(12).ToUnixTimeMilliseconds());
        }

        public async void UpdateVideo(Video video, Channel channel, IDataManager _dataManager)
        {
            // get all messages that were published
            var allSentMessages = await _redisDatabase.SetMembersAsync($"{YT_MSGS_SENT}-{video.Id}");
            var subscriptions = await _dataManager.GetSubscriptions(video, EventType.YoutubeVideoEdited);
            var subscriptionsDictionary = subscriptions.ToDictionary(k => k.Id, k => k);

            foreach (var messageSent in allSentMessages)
            {
                var dbMessageIdString = int.TryParse(messageSent.ToString().Split('-').ElementAt(0), out var dbSubscriptionId);
                if (!dbMessageIdString) continue;

                var discordMessageIdString = ulong.TryParse(messageSent.ToString().Split('-').ElementAt(1), out var discordMessageId);
                if (!discordMessageIdString) continue;

                var subscriptionExists = subscriptionsDictionary.TryGetValue(dbSubscriptionId, out var subscription);
                if (!subscriptionExists || subscription == null) continue;

                var subscriptionEvent = subscription.Events.FirstOrDefault();
                if (subscriptionEvent == null) continue;

                var templateHandler = TemplateHandler.InitiateYoutubeTemplateHandler(video, channel);

                var payload = new DiscordMessageCreatePayload
                {
                    Content = templateHandler.Parse(subscriptionEvent.Message.Content),
                    embeds = DiscordUtils.ConvertEntityEmbedToDiscordEmbed(subscriptionEvent.Message.Embeds, templateHandler)
                };

                // TODO: await this, if it throws an error (only on 400/404) then disable the subscription
                _discordRequestManager.UpdateWebhookMessage(payload, discordMessageId, subscription.WebhookUrl);
            }
        }

        public async Task<YoutubeNotification?> ConvertFromXml(Stream stream)
        {
            try
            {
                XNamespace atom = "http://www.w3.org/2005/Atom";
                XNamespace yt = "http://www.youtube.com/xml/schemas/2015";

                XDocument doc = XDocument.Load(stream);
                var entryElement = doc.Descendants(atom + "entry").FirstOrDefault();

                if (entryElement != null)
                {
                    YoutubeNotification notification = new YoutubeNotification
                    {
                        Id = entryElement.Element(atom + "id")?.Value,
                        VideoId = entryElement.Element(yt + "videoId")?.Value,
                        ChannelId = entryElement.Element(yt + "channelId")?.Value,
                        Title = entryElement.Element(atom + "title")?.Value,
                        Link = new Link
                        {
                            Href = entryElement.Element(atom + "link")?.Attribute("href")?.Value
                        },
                        Author = new Author
                        {
                            Name = entryElement.Element(atom + "author")?.Element(atom + "name")?.Value,
                            Uri = entryElement.Element(atom + "author")?.Element(atom + "uri")?.Value
                        },
                        Published = entryElement.Element(atom + "published")?.Value,
                        Updated = entryElement.Element(atom + "updated")?.Value
                    };

                    return notification;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while parsing XML: {ex.Message}");
            }

            return null;
        }

        public bool VerifyToken(string verifyToken) => verifyToken == IDENTIFIER!;
        public Task<bool> IsNewVideo(string videoId) => _redisDatabase.KeyExistsAsync($"{YT_SENT_VIDEOS}-{videoId}");

        public async Task AddResub(string channelId, ulong time)
        {
            await _redisDatabase.SortedSetAddAsync(YT_SUBS_EXPIRED, channelId, DateTimeOffset.UtcNow.AddSeconds(time).ToUnixTimeMilliseconds());

        }

        public string? GetYoutubeChannelId(string url)
        {
            // Match the URL
            Match match = YoutubeChannelRegex().Match(url);

            // Extract the channel ID
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                return null; // Return null if no match found
            }
        }

        /// <summary>
        /// Resubscribe to subscriptions that expired, subscription expire after 10d
        /// </summary>
        private void ResubTask()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(ONE_HOUR_MS);
                    var results = await _redisDatabase.SortedSetRangeByScoreAsync(YT_SUBS_EXPIRED, 0, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                    foreach (var result in results)
                    {
                        var channelId = result.ToString();
                        await _redisDatabase.SortedSetRemoveAsync(YT_SUBS_EXPIRED, channelId);
                        await Subscribe(channelId);
                        // wait 10s before trying to resub to not spam the hub and me
                        await Task.Delay(10000);
                    }
                }
            });
        }

        /// <summary>
        /// Delete all cached messages and sent videos that were cached more than 12h ago
        /// </summary>
        private void CleanupTask()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(ONE_HOUR_MS);
                    var expiredSets = await _redisDatabase.SortedSetRangeByScoreAsync(YT_SENT_VIDEOS, 0, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                    foreach (var expiredSet in expiredSets)
                    {
                        // delete the list storing the `subscriptionId-messageId` pairs for editing
                        await _redisDatabase.KeyDeleteAsync($"{YT_MSGS_SENT}-{expiredSet}");
                        // remove the videoId that expired (12h passed)
                        await _redisDatabase.SortedSetRemoveAsync(YT_SENT_VIDEOS, expiredSet.ToString());
                    }
                }
            });
        }

        [GeneratedRegex(@"https?:\/\/(?:www\.)?youtube\.com\/channel\/([a-zA-Z0-9_-]+)")]
        private static partial Regex YoutubeChannelRegex();
    }
}
