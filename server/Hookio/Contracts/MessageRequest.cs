using Discord;

namespace Hookio.Contracts
{
    public class MessageRequest
    {
        public string Content { get; set; }
        public Embed[] embed { get; set; }
        // TODO: make something that checks if the content + embed character count isn't above 6000
    }
}
