using Hookio.Enunms;

namespace Hookio.Contracts
{
    public class EventRequest
    {
        public int? Id { get; set; }
        public required MessageRequest Message { get; set; }
        public required EventType EventType { get; set; }
    }
}
