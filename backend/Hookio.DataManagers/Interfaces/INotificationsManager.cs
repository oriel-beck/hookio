using Google.Apis.YouTube.v3.Data;
using Hookio.Contracts.YouTube;
using Hookio.Data.Entities;

namespace Hookio.DataManagers.Interfaces
{
    public interface INotificationsManager
    {
        Task SendNotification(Video ytvideo, Channel ytchannel, YouTubeFeed ytfeed, string webhookUrl, Message message);
    }
}