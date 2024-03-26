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

    /// <summary>
    /// Converts the given string to a base64 string
    /// </summary>
    /// <param name="stringToEncode">The string to convert to base64</param>
    /// <param name="encoding">Encoding used while converting</param>
    /// <returns>A base64 encoded string</returns>
    public static string ToBase64(this string stringToEncode, Encoding encoding) => Convert.ToBase64String(encoding.GetBytes(stringToEncode));

    /// <summary>
    /// Converts the given base64 string to a normal string
    /// </summary>
    /// <param name="stringToDecode">The string to convert from base64</param>
    /// <param name="encoding">Encoding used while converting</param>
    /// <returns>A normal string</returns>
    public static string FromBase64(this string stringToDecode, Encoding encoding) => encoding.GetString(Convert.FromBase64String(str));
}
