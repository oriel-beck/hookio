﻿namespace Hookio.Utils.Interfaces
{
    public interface ITaskQueue
    {
        Task<HttpResponseMessage> Enqueue(int priority, Func<HttpClient, Task<HttpResponseMessage>> func);
    }
}