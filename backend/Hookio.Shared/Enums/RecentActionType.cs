namespace Hookio.Shared.Enums
{
    public enum RecentActionType
    {
        Success = 1, // sent a message to discord
        MissingWebhook, // attempted to send a message to discord and failed on webhook not found
        InvalidMessage, // attempted to send a message to discord and failed for invalid body or too long message
        UpdatedMessage // message was edited in the dashboard
    }
}
