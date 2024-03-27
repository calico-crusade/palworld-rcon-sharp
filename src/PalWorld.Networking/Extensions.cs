namespace PalWorld.Networking;

/// <summary>
/// Helpful extension methods for handling tasks
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Polyfill of .net 8's Task.WaitAsync
    /// </summary>
    /// <typeparam name="T">The type of return result from the task</typeparam>
    /// <param name="task">The task for which to wait on until completion.</param>
    /// <param name="timeout">The timeout after which the <see cref="Task"/> should be faulted with a <see cref="TimeoutException"/> if it hasn't otherwise completed.</param>
    /// <returns>The value of the task that completed before the timeout occurred</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="task"/> argument is null</exception>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="timeout"/> is negative without being <see cref="Timeout.InfiniteTimeSpan"/></exception>
    /// <exception cref="TimeoutException">Thrown if a timeout occurs</exception>
    /// <exception cref="TaskCanceledException">Thrown if the <paramref name="task"/> is cancelled</exception>
    /// <exception cref="InvalidOperationException">Thrown if the <paramref name="task"/> is faulted but no exception is provided</exception>
    public static async Task<T> WaitTimeout<T>(this Task<T> task, TimeSpan timeout)
    {
        //Most of this code was inspired by: https://github.com/dotnet/runtime/blob/204b10988f8adb12d4bd3fc7d313d41aeef19e39/src/libraries/Microsoft.Bcl.TimeProvider/src/System/Threading/Tasks/TimeProviderTaskExtensions.cs#L28

        //Ensure parameters are valid
        if (task is null) 
            throw new ArgumentNullException(nameof(task), "Task cannot be null");

        if (timeout != Timeout.InfiniteTimeSpan && timeout < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be non-negative");

        //Task already completed? Return it, no need to do anything else
        if (task.IsCompleted) return task.Result;
        //Timespan is infinite? Await the underlying task.
        if (timeout == Timeout.InfiniteTimeSpan) return await task;
        //Timespan is zero? Throw a timeout exception.
        if (timeout == TimeSpan.Zero) throw new TimeoutException("Timespan was zero");

        //Create a cancellation token for cancelling the delay if the task resolves
        using var delayCancelSource = new CancellationTokenSource();
        //Create the delay for the timeout.
        var delay = Task.Delay(timeout, delayCancelSource.Token);

        try
        {
            //Get the first task to complete
            var first = await Task.WhenAny(task, delay);
            //If the first task is the target, cancel the delay and return the result
            if (first == task)
            {
                delayCancelSource.Cancel();
                return task.Result;
            }

            //Throw the timeout exception
            throw new TimeoutException("Task did not complete within the specified timeout");
        }
        catch (OperationCanceledException)
        {
            //If the target task was cancelled, re-throw that exception
            if (task.IsCanceled) throw new TaskCanceledException(task);
            //If the target task was completed, but something went wrong with the delay (can be a race condition), return the result
            if (task.IsCompleted) return task.Result;
            //If the target task was faulted, throw the exception
            if (task.IsFaulted) throw task.Exception ?? throw new InvalidOperationException("Task faulted but has no exception");
            //Re-throw the cancellation in the event something went wrong with the delay task
            throw;
        }
    }

    /// <summary>
    /// Wrapping provider for <see cref="WaitTimeout{T}(Task{T}, TimeSpan)"/>
    /// </summary>
    /// <typeparam name="T">The type of return result from the task</typeparam>
    /// <param name="task">The task for which to wait on until completion.</param>
    /// <param name="timeout">The timeout (in milliseconds) after which the <see cref="Task"/> should be faulted with a <see cref="TimeoutException"/> if it hasn't otherwise completed.</param>
    /// <returns>The value of the task that completed before the timeout occurred</returns>
    public static async Task<T> WaitTimeout<T>(this Task<T> task, int timeout) => await task.WaitTimeout(TimeSpan.FromMilliseconds(timeout));
}
