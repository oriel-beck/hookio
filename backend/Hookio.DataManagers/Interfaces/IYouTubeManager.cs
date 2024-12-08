using Google.Apis.YouTube.v3.Data;
using Hookio.Contracts.YouTube;

namespace Hookio.DataManagers.Interfaces
{
    public interface IYouTubeManager
    {
        Task<HttpResponseMessage?> Subscribe(string channel_id, CancellationToken cancellationToken);
        YouTubeFeed ParseYouTubePayload(string xmlPayload);
        Task<Channel?> GetYouTubeChannelDetails(YouTubeSubscription subscription, CancellationToken cancellationToken);
        Task<Video?> GetYouTubeVideoDetails(YouTubeSubscription subscription, CancellationToken cancellationToken);
    }
}