using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Hookio.Contracts.YouTube;
using Hookio.DataManagers.Interfaces;
using Hookio.Shared.Options;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Security.Cryptography;
using System.Xml.Serialization;

namespace Hookio.DataManagers
{
    public class YouTubeManager(
        IHttpClientFactory httpClientFactory,
        IOptions<YouTube> youtubeOptions,
        IYouTubeSubscriptionCache youTubeSubscriptionCache,
        IConnectionMultiplexer connectionMultiplexer
        ) : IYouTubeManager
    {
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient("PubSubHubBub");
        private readonly YouTube _options = youtubeOptions.Value;
        private readonly IYouTubeSubscriptionCache _youTubeSubscriptionCache = youTubeSubscriptionCache;
        private readonly IDatabase _redis = connectionMultiplexer.GetDatabase();
        private readonly YouTubeService ytService = new(new BaseClientService.Initializer()
        {
            ApiKey = youtubeOptions.Value.Key,
            ApplicationName = "yt-announcements"
        });

        public async Task<HttpResponseMessage?> Subscribe(string channel_id, CancellationToken cancellationToken)
        {
            string token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            List<KeyValuePair<string, string>> formData = new()
                        {
                            { new("hub.callback", _options.HubCallback) },
                            { new("hub.topic", $"{_options.YouTubeFeedBaseUrl}{channel_id}") },
                            { new("hub.verify", "async") },
                            { new("hub.mode", "subscribe") },
                            { new("hub.lease_seconds", TimeSpan.FromDays(7).TotalSeconds.ToString() ) },
                            { new("hub.secret", _options.HubSecret) },
                            { new("hub.verify_token", token) }
                        };
            FormUrlEncodedContent httpContent = new(formData);
            var res = await _httpClient.PostAsync("/subscribe", httpContent, cancellationToken);
            if (res.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                // add a "pending" subscription to the cache
                _youTubeSubscriptionCache.Add(new() { ChannelId = channel_id, VerifyToken = token });
                // add a record so auto resubscribe can resubscribe
                await _redis.SortedSetAddAsync("hookio_yt_subs", channel_id, DateTimeOffset.UtcNow.AddDays(7).ToUnixTimeMilliseconds());
                return res;
            }
            else
            {
                return null;
            }
        }

        public YouTubeFeed ParseYouTubePayload(string xmlPayload)
        {
            var serializer = new XmlSerializer(typeof(YouTubeFeed));

            using var stringReader = new StringReader(xmlPayload);
            return (YouTubeFeed)serializer.Deserialize(stringReader)!;
        }

        public async Task<Channel?> GetYouTubeChannelDetails(YouTubeSubscription subscription, CancellationToken cancellationToken)
        {
            if (subscription.Channel != null) return subscription.Channel;
            var channelRequest = ytService.Channels.List("snippet, statistics");
            channelRequest.Id = subscription.ChannelId;
            var channelList = await channelRequest.ExecuteAsync(cancellationToken);

            var channel = channelList.Items.FirstOrDefault();
            if (channel == null) return null;

            subscription.Channel = channel;
            _youTubeSubscriptionCache.Update(subscription);
            
            return channel;
        }

        public async Task<Video?> GetYouTubeVideoDetails(YouTubeSubscription subscription, string videoId, CancellationToken cancellationToken)
        {
            if (subscription.LatestVideo != null && subscription.LatestVideo.Id == videoId) return subscription.LatestVideo;
            var videoListRequest = ytService.Videos.List("snippet, contentDetails");
            videoListRequest.Id = videoId;

            var videoList = await videoListRequest.ExecuteAsync(cancellationToken);
            if (videoList == null) return null;

            var video = videoList.Items.FirstOrDefault();
            if (video == null) return null;

            subscription.LatestVideo = video;
            _youTubeSubscriptionCache.Update(subscription);

            return video;
        }
    }
}
