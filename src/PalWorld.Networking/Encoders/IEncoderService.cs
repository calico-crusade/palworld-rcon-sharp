
namespace PalWorld.Networking;

/// <summary>
/// Represents a service that can encode and decode packet data
/// </summary>
public interface IEncoderService
{
    /// <summary>
    /// Encodes the given data into a byte array
    /// </summary>
    /// <param name="data">The data to be encoded</param>
    /// <returns>The encoded data</returns>
    byte[] GetBytes(string data);

    /// <summary>
    /// Decodes the given data from a byte array
    /// </summary>
    /// <param name="data">The data to decode</param>
    /// <returns>The decoded data</returns>
    string GetString(byte[] data);

    /// <summary>
    /// Decodes the given data from a byte array
    /// </summary>
    /// <param name="data">The data to decode</param>
    /// <param name="index">The index to start at</param>
    /// <param name="count">The count of the data to decode</param>
    /// <returns>The returned data</returns>
    string GetString(byte[] data, int index, int count);
}
