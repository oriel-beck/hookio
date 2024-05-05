using System.Text.Json.Serialization;

namespace Hookio.Discord.Contracts
{
    [method: JsonConstructor]
    public class DiscordPartialMessage(ulong id)
    {
        public ulong Id { get; } = id;
        
    }
}
