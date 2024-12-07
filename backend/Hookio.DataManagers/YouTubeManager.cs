using Hookio.DataManagers.Interfaces;
using Hookio.Shared.Options;
using Microsoft.Extensions.Options;

namespace Hookio.DataManagers
{
    public class YouTubeManager(
        IHttpClientFactory httpClientFactory, 
        IOptions<YouTube> youtubeOptions,
        IYouTubeSubscriptionCache youTubeSubscriptionCache
        ) : IYouTubeManager
    {
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient("PubSubHubBub");
        private readonly YouTube _options = youtubeOptions.Value;
        private readonly IYouTubeSubscriptionCache _youTubeSubscriptionCache = youTubeSubscriptionCache;

        public async Task<HttpResponseMessage?> Subscribe(string channel_id, CancellationToken cancellationToken)
        {
            List<KeyValuePair<string, string>> formData = new()
                        {
                { new("hub.callback", _options.HubCallback) },
                            { new("hub.topic", $"{_options.YouTubeFeedBaseUrl}{channel_id}") },
                            { new("hub.verify", "async") },
                            { new("hub.mode", "subscribe") },
                            { new("hub.lease_seconds", TimeSpan.FromDays(7).TotalSeconds.ToString() ) },
                            { new("hub.secret", _options.HubSecret) }
                        };
            FormUrlEncodedContent httpContent = new(formData);
            var res = await _httpClient.PostAsync("/subscribe", httpContent, cancellationToken);
            if (res.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                // add a "pending" subscription to the cache
                await _youTubeSubscriptionCache.AddAsync(new() { TopicUrl = $"{_options.YouTubeFeedBaseUrl}{channel_id}" });
                return res;
            }
            else
            {
                return null;
            }
        }
    }
}
