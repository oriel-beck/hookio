using System;
namespace Hookio.Shared.Enums
{
    public enum MessageAction
    {
        // can be used for any message type
        CreateMessage = 1, // creates a new message, abandoning the old one
        // can only be used for "update" MessageType's
        UpdateMessage, // updates the original message
        // can only be used for "delete/end" MessageType's
        DeleteMessage // deletes the original message
    }
}
