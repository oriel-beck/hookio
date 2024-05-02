namespace Hookio.Discord
{
    public class DiscordTask(int priority, Func<Task<HttpResponseMessage>> task)
    {
        public readonly TaskCompletionSource<HttpResponseMessage> _tcs = new();
        public readonly Func<Task<HttpResponseMessage>> _httpTask = task;
        public readonly int _priority = priority;
    }
}
