using Hookio.Shared.Enums;

namespace Hookio.Contracts.RecentAction
{
    public class RecentActionResponse
    {
        public required int Id { get; set; }

        public required RecentActionType Type { get; set; }

        public required DateTime CreatedAt { get; set; }

        // The action that failed (SendMessage/DeleteMessage/UpdateMessage)
        public required MessageAction Action { get; set; }
    }
}
