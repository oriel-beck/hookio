using Google.Apis.YouTube.v3.Data;
using Hookio.Database.Interfaces;
using Hookio.Utils.Contracts;

namespace Hookio.Utils.Interfaces
{
    public interface IYoutubeService
    {
        public Task<bool?> Subscribe(string channelId, bool subscribe = true);
        public bool VerifyToken(string verifyToken);
        public Task<YoutubeNotification?> ConvertFromXml(Stream stream);
        public void PublishVideo(Video video, Channel channel, IDataManager dataManager);
        public void UpdateVideo(Video video, Channel channel, IDataManager dataManager);
        public string? GetYoutubeChannelId(string url);
        public Task AddResub(string channelId, ulong time);
        public Task<long?> IsNewVideo(string videoId);

    }
}
