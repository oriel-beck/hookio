using Hookio.DataManagers.Utils.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hookio.Utils
{
    public class DiscordTask(int priority, Func<HttpClient, Task<HttpResponseMessage>> task)
    {
        public readonly TaskCompletionSource<HttpResponseMessage> _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public readonly Func<HttpClient, Task<HttpResponseMessage>> _httpTask = task;
        public readonly int _priority = priority;
    }

    /// <summary>
    /// Priority Discord HTTP queue. Uses X-RateLimit-Reset-After / retry_after per
    /// https://discord.com/developers/docs/topics/rate-limits
    /// </summary>
    public class TaskQueue : BackgroundService, ITaskQueue
    {
        private readonly ConcurrentDictionary<int, ConcurrentQueue<DiscordTask>> _queue = new(new Dictionary<int, ConcurrentQueue<DiscordTask>>
        {
            { 0, new ConcurrentQueue<DiscordTask>() },
            { 1, new ConcurrentQueue<DiscordTask>() },
            { 2, new ConcurrentQueue<DiscordTask>() }
        });

        private readonly ConcurrentDictionary<string, DateTimeOffset> _bucketAvailableAt = new();
        private readonly SemaphoreSlim _signal = new(0);
        private readonly Queue<DateTimeOffset> _recentRequests = new();
        private DateTimeOffset _globalAvailableAt = DateTimeOffset.MinValue;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<TaskQueue> _logger;

        public TaskQueue(IHttpClientFactory httpClientFactory, ILogger<TaskQueue> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public Task<HttpResponseMessage> Enqueue(int priority, Func<HttpClient, Task<HttpResponseMessage>> func)
        {
            if (!_queue.ContainsKey(priority))
            {
                throw new ArgumentException("Invalid priority level.");
            }

            var task = new DiscordTask(priority, func);
            _queue[priority].Enqueue(task);
            _signal.Release();
            return task._tcs.Task;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var work = PeekHighestPriorityTask();
                if (work is null)
                {
                    try
                    {
                        await _signal.WaitAsync(TimeSpan.FromMilliseconds(100), stoppingToken);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }
                    continue;
                }

                var (priority, task, bucketKey) = work.Value;
                if (!CanExecuteGlobally())
                {
                    await Task.Delay(50, stoppingToken);
                    continue;
                }

                try
                {
                    var client = _httpClientFactory.CreateClient();
                    client.BaseAddress = new Uri("https://discord.com");
                    HttpResponseMessage response;
                    while (true)
                    {
                        stoppingToken.ThrowIfCancellationRequested();
                        response = await task._httpTask(client);
                        if ((int)response.StatusCode != 429) break;

                        var bodyJson = await response.Content.ReadAsStringAsync(stoppingToken);
                        var retryAfter = GetRetryAfter(response, bodyJson);
                        var scope = GetHeader(response, "X-RateLimit-Scope");
                        var globalBody = false;
                        try
                        {
                            globalBody = JsonSerializer.Deserialize<DiscordRateLimitBody>(bodyJson)?.Global == true;
                        }
                        catch { /* ignore malformed 429 bodies */ }

                        if (scope == "global" || globalBody)
                        {
                            _globalAvailableAt = DateTimeOffset.UtcNow + retryAfter;
                        }
                        else
                        {
                            var bucket = GetHeader(response, "X-RateLimit-Bucket") ?? bucketKey;
                            _bucketAvailableAt[bucket] = DateTimeOffset.UtcNow + retryAfter;
                        }
                        _logger.LogInformation("Discord 429, waiting {RetryAfter}s", retryAfter.TotalSeconds);
                        await Task.Delay(retryAfter, stoppingToken);
                    }

                    UpdateBucketFromHeaders(response, bucketKey);
                    RecordGlobalRequest();
                    Dequeue(priority);
                    if ((int)response.StatusCode >= 400)
                    {
                        _logger.LogWarning("Discord request failed with {Status}", response.StatusCode);
                    }
                    task._tcs.TrySetResult(response);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Dequeue(priority);
                    task._tcs.TrySetException(ex);
                    _logger.LogError(ex, "Discord queue task failed");
                }
            }
        }

        private (int Priority, DiscordTask Task, string BucketKey)? PeekHighestPriorityTask()
        {
            if (DateTimeOffset.UtcNow < _globalAvailableAt) return null;

            foreach (var kvp in _queue.OrderBy(k => k.Key))
            {
                if (kvp.Value.IsEmpty) continue;
                if (!kvp.Value.TryPeek(out var task) || task is null) continue;

                var bucketKey = $"priority:{kvp.Key}";
                if (_bucketAvailableAt.TryGetValue(bucketKey, out var availableAt) && DateTimeOffset.UtcNow < availableAt)
                {
                    continue;
                }

                return (kvp.Key, task, bucketKey);
            }

            return null;
        }

        private void Dequeue(int priority) => _queue.GetValueOrDefault(priority)?.TryDequeue(out _);

        private bool CanExecuteGlobally()
        {
            if (DateTimeOffset.UtcNow < _globalAvailableAt) return false;
            var cutoff = DateTimeOffset.UtcNow.AddSeconds(-1);
            while (_recentRequests.Count > 0 && _recentRequests.Peek() < cutoff)
            {
                _recentRequests.Dequeue();
            }
            return _recentRequests.Count < 50;
        }

        private void RecordGlobalRequest() => _recentRequests.Enqueue(DateTimeOffset.UtcNow);

        private void UpdateBucketFromHeaders(HttpResponseMessage response, string fallbackKey)
        {
            var remaining = GetHeader(response, "X-RateLimit-Remaining");
            if (!int.TryParse(remaining, out var remainingCount) || remainingCount > 0) return;

            var resetAfter = ParseResetAfter(response);
            if (resetAfter is null) return;

            var bucket = GetHeader(response, "X-RateLimit-Bucket") ?? fallbackKey;
            _bucketAvailableAt[bucket] = DateTimeOffset.UtcNow + resetAfter.Value;
        }

        private static TimeSpan GetRetryAfter(HttpResponseMessage response, string bodyJson)
        {
            var resetAfter = ParseResetAfter(response);
            if (resetAfter is not null) return resetAfter.Value;

            var retryAfterHeader = GetHeader(response, "Retry-After");
            if (double.TryParse(retryAfterHeader, NumberStyles.Float, CultureInfo.InvariantCulture, out var retryAfterSeconds))
            {
                return TimeSpan.FromSeconds(retryAfterSeconds);
            }

            try
            {
                var body = JsonSerializer.Deserialize<DiscordRateLimitBody>(bodyJson);
                if (body is not null && body.RetryAfter > 0)
                {
                    return TimeSpan.FromSeconds(body.RetryAfter);
                }
            }
            catch
            {
                // fall through
            }

            return TimeSpan.FromSeconds(1);
        }

        private static TimeSpan? ParseResetAfter(HttpResponseMessage response)
        {
            var value = GetHeader(response, "X-RateLimit-Reset-After");
            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds) && seconds > 0)
            {
                return TimeSpan.FromSeconds(seconds);
            }
            return null;
        }

        private static string? GetHeader(HttpResponseMessage response, string name) =>
            response.Headers.TryGetValues(name, out var values) ? values.FirstOrDefault() : null;

        private sealed class DiscordRateLimitBody
        {
            [JsonPropertyName("retry_after")]
            public double RetryAfter { get; set; }

            [JsonPropertyName("global")]
            public bool Global { get; set; }
        }
    }
}
