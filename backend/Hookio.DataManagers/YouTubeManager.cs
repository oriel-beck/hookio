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
    /// <summary>
    /// Manages YouTube subscriptions.<br/>
    /// Creates YT subscriptions.<br/>
    /// Gets YT channel and video data.<br/>
    /// Parses incoming notifications payload.
    /// </summary>
    /// <param name="httpClientFactory"></param>
    /// <param name="youtubeOptions"></param>
    /// <param name="youTubeSubscriptionCache"></param>
    /// <param name="connectionMultiplexer"></param>
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

        /// <summary>
        /// Sends a subscriptions request to the pubsubhubbub hub
        /// </summary>
        /// <param name="channel_id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Parses incoming notifications from pubsubhubbub hub from XML to YouTubeFeed
        /// </summary>
        /// <param name="xmlPayload"></param>
        /// <returns></returns>
        public YouTubeFeed ParseYouTubePayload(string xmlPayload)
        {
            var serializer = new XmlSerializer(typeof(YouTubeFeed));

            using var stringReader = new StringReader(xmlPayload);
            return (YouTubeFeed)serializer.Deserialize(stringReader)!;
        }

        /// <summary>
        /// Gets YouTube channel data from cache or YouTube (uses "quota")<br/>
        /// Resource: https://developers.google.com/youtube/v3/docs/channels#resource
        /// </summary>
        /// <param name="subscription"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Channel?> GetYouTubeChannelDetails(YouTubeSubscription subscription, CancellationToken cancellationToken)
        {
            if (subscription.Channel != null) return subscription.Channel;
            var channelRequest = ytService.Channels.List("snippet,statistics");
            channelRequest.Id = subscription.ChannelId;
            var channelList = await channelRequest.ExecuteAsync(cancellationToken);

            var channel = channelList.Items.FirstOrDefault();
            if (channel == null) return null;

            subscription.Channel = channel;
            _youTubeSubscriptionCache.Update(subscription);
            
            return channel;
        }

        /// <summary>
        /// Gets YouTube video data from cache or YouTube (uses "quota")<br/>
        /// Resource: https://developers.google.com/youtube/v3/docs/videos#resource
        /// </summary>
        /// <param name="subscription"></param>
        /// <param name="videoId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Video?> GetYouTubeVideoDetails(YouTubeSubscription subscription, string videoId, CancellationToken cancellationToken)
        {
            // only return cached data if the latest video saved has the same Id as the currently sending video
            if (subscription.LatestVideo != null && subscription.LatestVideo.Id == videoId) return subscription.LatestVideo;
            var videoListRequest = ytService.Videos.List("snippet,contentDetails,statistics");
            videoListRequest.Id = videoId;

            var videoList = await videoListRequest.ExecuteAsync(cancellationToken);
            if (videoList == null) return null;

            var video = videoList.Items.FirstOrDefault();
            if (video == null) return null;

            subscription.LatestVideo = video;
            _youTubeSubscriptionCache.Update(subscription);

            return video;
        }

        /// <summary>
        /// Generates the template string key-value for converting {templateString} to its value
        /// </summary>
        /// <param name="video"></param>
        /// <param name="channel"></param>
        /// <param name="feed"></param>
        /// <returns></returns>
        public Dictionary<string, string> GetTemplateStrings(Video video, Channel channel, YouTubeFeed feed)
        {
            var videoSnippet = video.Snippet;

            var channelSnippet = channel.Snippet;
            var channelStatistics = channel.Statistics;
            Dictionary<string, string> res = new()
            {
                { "video.url", feed.Entry.Link.Href },
                { "video.description", videoSnippet.Description },
                { "video.title", videoSnippet.Title },
                { "video.thumbnail.default", videoSnippet.Thumbnails.Standard.Url },
                { "video.thumbnail.medium", videoSnippet.Thumbnails.Medium.Url },
                { "video.thumbnail.high", videoSnippet.Thumbnails.High.Url },
                
                { "channel.title", channelSnippet.Title },
                { "channel.name", channelSnippet.Title },
                { "channel.url", channelSnippet.CustomUrl ?? feed.Entry.Author.Uri },
                { "channel.thumbnail.default", channelSnippet.Thumbnails.Standard.Url },
                { "channel.thumbnail.medium", channelSnippet.Thumbnails.Medium.Url },
                { "channel.thumbnail.high", channelSnippet.Thumbnails.High.Url },
                { "channel.subscribers", channelStatistics.SubscriberCount.ToString() ?? "0" },
                { "channel.views", channelStatistics.VideoCount.ToString() ?? "0" },

                { "everyone", "@everyone" },
                { "here", "@here" }
            };
            return res;
        }
    }
}
