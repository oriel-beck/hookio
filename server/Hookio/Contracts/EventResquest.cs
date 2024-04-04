using Hookio.Enunms;

namespace Hookio.Contracts
{
    public class EventRequest
    {
        public int? Id { get; set; }
        public MessageRequest Message { get; set; }
        public EventType EventType { get; set; }
    }
}
