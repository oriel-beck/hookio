using Hookio.Youtube.Contracts;

namespace Hookio.Youtube.Interfaces
{
    public interface IYoutubeService
    {
        public Task<bool?> Subscribe(string channelId, bool subscribe = true);
        public bool VerifyToken(string verifyToken);
        public YoutubeNotification ConvertAtomToSyndication(Stream stream);
        public void PublishVideo(YoutubeNotification notification);
        public void UpdateVideo(YoutubeNotification notification);
        public string? GetYoutubeChannelId(string url);
    }
}
