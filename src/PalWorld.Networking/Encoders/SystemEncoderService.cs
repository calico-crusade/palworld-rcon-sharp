namespace PalWorld.Networking;

/// <summary>
/// Represents an encoder service that uses a specific <see cref="Encoding"/> for encoding and decoding
/// </summary>
/// <param name="_encoding"></param>
public abstract class SystemEncoderService(Encoding _encoding) : IEncoderService
{
    /// <summary>
    /// The <see cref="Encoding"/> to use for encoding and decoding
    /// </summary>
    public virtual Encoding Encoding { get; } = _encoding;

    /// <summary>
    /// Encodes the given data into a byte array
    /// </summary>
    /// <param name="data">The data to be encoded</param>
    /// <returns>The encoded data</returns>
    public virtual byte[] GetBytes(string data)
    {
        return Encoding.GetBytes(data);
    }

    /// <summary>
    /// Encodes the given data into a byte array
    /// </summary>
    /// <param name="data">The data to be encoded</param>
    /// <returns>The encoded data</returns>
    public virtual string GetString(byte[] data)
    {
        return Encoding.GetString(data);
    }

    /// <summary>
    /// Decodes the given data from a byte array
    /// </summary>
    /// <param name="data">The data to decode</param>
    /// <param name="index">The index to start at</param>
    /// <param name="count">The count of the data to decode</param>
    /// <returns>The returned data</returns>
    public virtual string GetString(byte[] data, int index, int count)
    {
        return Encoding.GetString(data, index, count);
    }
}

/// <summary>
/// Represents an encoder service that uses UTF-8 for encoding and decoding
/// </summary>
public class Utf8EncoderService() : SystemEncoderService(Encoding.UTF8) { }

/// <summary>
/// Represents an encoder service that uses UTF-16 for encoding and decoding
/// </summary>
public class Utf16EncoderService() : SystemEncoderService(Encoding.Unicode) { }

/// <summary>
/// Represents an encoder service that uses UTF-32 for encoding and decoding
/// </summary>
public class Utf32EncoderService() : SystemEncoderService(Encoding.UTF32) { }