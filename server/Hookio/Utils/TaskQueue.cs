using Hookio.Utils.Interfaces;
using System.Collections.Concurrent;

namespace Hookio.Utils
{
    public class TaskQueue : ITaskQueue
    {
        private readonly ConcurrentDictionary<int, ConcurrentQueue<DiscordTask>> _queue = new(new Dictionary<int, ConcurrentQueue<DiscordTask>>
        {
            {0, new ConcurrentQueue<DiscordTask>() },
            {1, new ConcurrentQueue<DiscordTask>() },
            {2, new ConcurrentQueue<DiscordTask>() }
        });

        private readonly Dictionary<int, DateTimeOffset> _ratelimits = [];
        private DateTime _lastTaskExecution = DateTime.MinValue;
        private int _tasksExecutedThisSecond = 0;
        private DateTimeOffset _globalRatelimitReset = DateTimeOffset.MinValue;
        private IHttpClientFactory _httpClientFactory;

        public TaskQueue(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            ProcessQueueAsync();
            ResetTasksAsync();
        }

        private void ResetTasksAsync()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(1000); // Wait for 1 second
                    ResetTasksExecutedThisSecond();
                }
            });
        }

        private void ProcessQueueAsync()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    var task = GetHighestPriorityTask();
                    if (task == null) continue;
                    var (priority, taskToExecute) = task.Value;
                    try
                    {
                        var client = _httpClientFactory.CreateClient();
                        client.BaseAddress = new Uri("https://discord.com");
                        var response = await taskToExecute._httpTask(client);
                        response.EnsureSuccessStatusCode();
                        // if it succeeded, dequeue
                        taskToExecute._tcs.SetResult(response);
                        _queue.GetValueOrDefault(priority)?.TryDequeue(out var _);
                        UpdateRatelimit(response, priority);
                    }
                    catch (HttpRequestException ex)
                    {
                        Console.WriteLine(ex.ToString());
                        // if status code is not 429, dequeue, otherwise, try to execute this again after the ratelimit ends
                        if ((int)(ex.StatusCode is not null ? ex.StatusCode : 0) != 429)
                        {
                            _queue.GetValueOrDefault(priority)?.TryDequeue(out var _);
                            taskToExecute._tcs.SetException(ex);
                        }
                    }
                }
            });
        }

        private void ResetTasksExecutedThisSecond() =>
            _tasksExecutedThisSecond = DateTime.UtcNow - _lastTaskExecution > TimeSpan.FromSeconds(1) ? 0 : _tasksExecutedThisSecond;

        private bool CanExecuteTask() =>
            _tasksExecutedThisSecond < 50 || DateTime.UtcNow - _lastTaskExecution > TimeSpan.FromSeconds(1);

        private (int, DiscordTask)? GetHighestPriorityTask()
        {
            // stop all executions in case of a global ratelimit hit (should never happen, but can't be too careful)
            if (_globalRatelimitReset >= DateTimeOffset.UtcNow) return null;
            foreach (var kvp in _queue)
            {
                if (_ratelimits.TryGetValue(kvp.Key, out var priorityValue) && priorityValue >= DateTimeOffset.UtcNow)
                {
                    continue; // Ratelimited, move to the next priority
                }

                if (!kvp.Value.IsEmpty && CanExecuteTask())
                {
                    _tasksExecutedThisSecond++;
                    _lastTaskExecution = DateTime.UtcNow;
                    kvp.Value.TryPeek(out var result);
                    if (result == null) continue;
                    return (kvp.Key, result);
                }
            }

            return null;
        }

        private void UpdateRatelimit(HttpResponseMessage response, int priority)
        {
            var headers = response.Headers;

            if (headers.TryGetValues("x-ratelimit-remaining", out var ratelimitRemainingValues) &&
                ratelimitRemainingValues.FirstOrDefault() is string ratelimitRemainingString &&
                int.TryParse(ratelimitRemainingString, out var ratelimitRemaining) &&
                ratelimitRemaining == 0 &&
                headers.TryGetValues("x-ratelimit-reset", out var resetAfterValues) &&
                resetAfterValues.FirstOrDefault() is string resetAfterString &&
                double.TryParse(resetAfterString, out var resetAfter))
            {
                if ((int)response.StatusCode == 429 && headers.TryGetValues("x-ratelimit-scope", out var scopeValues))
                {
                    if (scopeValues.FirstOrDefault() == "global")
                    {
                        _globalRatelimitReset = DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(resetAfter));
                    }
                }
                else
                {
                    _ratelimits[priority] = DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(resetAfter));
                }
            }
        }

        public Task<HttpResponseMessage> Enqueue(int priority, Func<HttpClient, Task<HttpResponseMessage>> func)
        {
            DiscordTask task = new(priority, func);
            if (!_queue.ContainsKey(task._priority))
            {
                throw new ArgumentException("Invalid priority level.");
            }

            _queue[task._priority].Enqueue(task);
            return task._tcs.Task;
        }
    }
}
