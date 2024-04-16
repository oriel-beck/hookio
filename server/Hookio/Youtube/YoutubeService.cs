﻿using Hookio.Database.Interfaces;
using Hookio.Discord.Interfaces;
using Hookio.Enunms;
using Hookio.Youtube.Contracts;
using Hookio.Youtube.Interfaces;
using StackExchange.Redis;
using System.ServiceModel.Syndication;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace Hookio.Youtube
{
    public class YoutubeService : IYoutubeService
    {
        private readonly string? IDENTIFIER = Environment.GetEnvironmentVariable("YT_IDENTIFIER");
        const string YT_CALLBACK_URL = "https://hookio.gg/api/youtube/callback";
        // Set of channelId : score (timestamp)
        const string YT_SUBS_EXPIRED = "youtube_subscriptions_expiration";
        // Set of `{YT_SENT_VIDEOS}-videoId` : dbMessageId-messageId
        const string YT_MSGS_SENT = "youtube_messages_sent";
        // Set of videoId : score (timestamp)
        const string YT_SENT_VIDEOS = "youtube_videos_sent";
        private readonly HttpClient _httpClient = new();
        private readonly IDatabase _database;
        private readonly IDataManager _dataManager;
        private readonly IDiscordClientManager _discordClientManager;

        public YoutubeService(IConnectionMultiplexer connectionMultiplexer, IDataManager dataManager, IDiscordClientManager discordClientManager) 
        {
            _database = connectionMultiplexer.GetDatabase();
            _dataManager = dataManager;
            _discordClientManager = discordClientManager;
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
                new("hub.callback", HttpUtility.UrlEncode(YT_CALLBACK_URL)),
                new("hub.topic", HttpUtility.UrlEncode($"https://www.youtube.com/xml/feeds/videos.xml?channel_id={channelId}")),
            ]);

            var res = await _httpClient.PostAsync("https://pubsubhubbub.appspot.com/subscribe", content);
            if (res.IsSuccessStatusCode) return null;
            await _database.SortedSetAddAsync(YT_SUBS_EXPIRED, channelId, DateTimeOffset.UtcNow.AddDays(10).ToUnixTimeMilliseconds());
            return true;
        }
        public async void PublishVideo(YoutubeNotification notification)
        {
            // get all subscriptions for this channel
            var subscriptions = await _dataManager.GetSubscriptions(notification);
            foreach (var subscription in subscriptions)
            {
                var ev = subscription.Events.Find(ev => ev.Type == EventType.YoutubeVideoUploaded);
                if (ev == null) continue;
                _discordClientManager.SendWebhookAsync(ev.Message, subscription.WebhookUrl, notification.VideoId);
            }
            // add the video ID to the sorted set to be deleted in 12h
            await _database.SortedSetAddAsync(YT_SENT_VIDEOS, notification.VideoId, DateTimeOffset.UtcNow.AddHours(12).ToUnixTimeMilliseconds());
        }

        public async void UpdateVideo(YoutubeNotification notification)
        {
            // get all messages that were published
            var allSentMessages = await _database.SetMembersAsync($"{YT_MSGS_SENT}-{notification.VideoId}");
            foreach (var messageSent in allSentMessages)
            {
                var subscriptionId = messageSent.ToString().Split('-').GetValue(0);
                var messageId = messageSent.ToString().Split('-').GetValue(1);
                var subscription = await _dataManager.GetSubscription((int)subscriptionId!);
                if (subscription == null) continue;
                var ev = subscription.Events.Find(ev => ev.Type == EventType.YoutubeVideoEdited);
                if (ev == null) continue;
                _discordClientManager.EditWebhookAsync(ev.Message, (ulong)messageId!, subscription.WebhookUrl);
            }
        }


        public YoutubeNotification ConvertAtomToSyndication(Stream stream)
        {
            using var xmlReader = XmlReader.Create(stream);
            SyndicationFeed feed = SyndicationFeed.Load(xmlReader);
            var item = feed.Items.FirstOrDefault();
            return new YoutubeNotification()
            {
                ChannelId = GetElementExtensionValueByOuterName(item!, "channelId")!,
                VideoId = GetElementExtensionValueByOuterName(item!, "videoId")!,
                Title = item!.Title.Text,
                Published = item!.PublishDate.ToString("dd/MM/yyyy"),
                Updated = item!.LastUpdatedTime.ToString("dd/MM/yyyy")
            };
        }

        public bool VerifyToken(string verifyToken) => verifyToken == IDENTIFIER!;

        private static string? GetElementExtensionValueByOuterName(SyndicationItem item, string outerName)
        {
            if (item.ElementExtensions.All(x => x.OuterName != outerName)) return null;
            return item.ElementExtensions.Single(x => x.OuterName == outerName).GetObject<XElement>().Value;
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
                    await Task.Delay(3600000);
                    var results = await _database.SortedSetRangeByScoreAsync(YT_SUBS_EXPIRED, 0, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                    foreach (var result in results)
                    {
                        var channelId = result.ToString();
                        await _database.SortedSetRemoveAsync(YT_SUBS_EXPIRED, channelId);
                        await Subscribe(channelId);
                        // wait 10s before trying to resub to not spam the hub and me
                        await Task.Delay(10000);
                    }
                }
            });
        }

        /// <summary>
        /// Delete all 
        /// </summary>
        private void CleanupTask()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(3600000);
                    var expiredSets = await _database.SortedSetRangeByScoreAsync(YT_SENT_VIDEOS, 0, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                    foreach (var expiredSet in expiredSets)
                    {
                        // delete the list storing the `subscriptionId-messageId` pairs for editing
                        await _database.KeyDeleteAsync($"{YT_MSGS_SENT}-{expiredSet}");
                        // remove the videoId that expired (12h passed)
                        await _database.SortedSetRemoveAsync(YT_SENT_VIDEOS, expiredSet.ToString());
                    }
                }
            });
        }
    }
}
