using Hookio.Enunms;

namespace Hookio.Contracts
{
    public class EventRequest
    {
        public MessageRequest Message { get; set; }
        public EventType EventType { get; set; }
    }
}
