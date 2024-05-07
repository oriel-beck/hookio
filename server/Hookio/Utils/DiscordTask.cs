namespace Hookio.Utils
{
    public class DiscordTask(int priority, Func<HttpClient, Task<HttpResponseMessage>> task)
    {
        public readonly TaskCompletionSource<HttpResponseMessage> _tcs = new();
        public readonly Func<HttpClient, Task<HttpResponseMessage>> _httpTask = task;
        public readonly int _priority = priority;
    }
}
