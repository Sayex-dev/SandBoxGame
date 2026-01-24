using System;
using System.Threading.Tasks;

public static class TaskExtensions
{
    public static void FireAndForget(
        this Task task,
        Action<Exception>? onException = null)
    {
        task.ContinueWith(t =>
        {
            if (t.Exception != null)
                onException?.Invoke(t.Exception);
        }, TaskContinuationOptions.OnlyOnFaulted);
    }
}