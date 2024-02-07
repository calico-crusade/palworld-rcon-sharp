namespace PalWorld.Networking;

/// <summary>
/// Helpful extension methods for handling tasks
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Polyfill of .net 8's Task.WaitAsync
    /// </summary>
    /// <typeparam name="T">The type of task</typeparam>
    /// <param name="task">The task operation</param>
    /// <param name="timeoutMs">How long to wait before timing out</param>
    /// <returns>The value of the task</returns>
    /// <exception cref="TimeoutException">Thrown if the timeout occurs</exception>
    public static async Task<T> WaitTimeout<T>(this Task<T> task, int timeoutMs)
    {
        var operation = await Task.WhenAny(task, Task.Delay(timeoutMs));
        if (operation == task) return await task;

        throw new TimeoutException();
    }
}
