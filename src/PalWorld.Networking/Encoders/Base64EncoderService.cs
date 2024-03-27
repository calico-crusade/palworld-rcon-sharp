namespace PalWorld.Networking;

/// <summary>
/// Represents a service that can encode and decode packet data using Base64
/// </summary>
public class Base64EncoderService : IEncoderService
{
    /// <summary>
    /// Encodes the given data into a byte array
    /// </summary>
    /// <param name="data">The data to be encoded</param>
    /// <returns>The encoded data</returns>
    public byte[] GetBytes(string data)
    {
        return Convert.FromBase64String(data);
    }

    /// <summary>
    /// Decodes the given data from a byte array
    /// </summary>
    /// <param name="data">The data to decode</param>
    /// <returns>The decoded data</returns>
    public string GetString(byte[] data)
    {
        return Convert.ToBase64String(data);
    }

    /// <summary>
    /// Decodes the given data from a byte array
    /// </summary>
    /// <param name="data">The data to decode</param>
    /// <param name="index">The index to start at</param>
    /// <param name="count">The count of the data to decode</param>
    /// <returns>The returned data</returns>
    public string GetString(byte[] data, int index, int count)
    {
        return Convert.ToBase64String(data, index, count);
    }
}
