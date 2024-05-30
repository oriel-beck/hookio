using Hookio.Enunms;

namespace Hookio.Contracts
{
    public class EventResponse
    {
        public int Id { get; set; }
        public MessageResponse Message { get; set; }
        public EventType EventType { get; set; }
    }
}
