using Hookio.Enunms;

namespace Hookio.Contracts
{
    public class EventResponse
    {
        public required int Id { get; set; }
        public required MessageResponse Message { get; set; }
        public required EventType EventType { get; set; }
    }
}
