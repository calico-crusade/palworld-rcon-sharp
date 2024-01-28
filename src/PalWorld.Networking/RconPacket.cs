namespace PalWorld.Networking;

/// <summary>
/// Represents data sent across the network for RCON data
/// </summary>
public class RconPacket
{
    /// <summary>
    /// The ID of the packet (used for matching requests to responses)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The raw type of packet
    /// </summary>
    public int RawType { get; set; }

    /// <summary>
    /// The type of packet
    /// </summary>
    public RconPacketType Type => (RconPacketType)RawType;

    /// <summary>
    /// The content of the packet
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// Creates a new <see cref="RconPacket"/> from the given data
    /// </summary>
    /// <param name="id">The ID of the packet (used for matching requests to responses)</param>
    /// <param name="type">The type of packet</param>
    /// <param name="content">The content of the packet</param>
    public RconPacket(int id, int type, string content)
    {
        Id = id;
        RawType = type;
        Content = content;
    }

    /// <summary>
    /// Creates a new <see cref="RconPacket"/> from the given data
    /// </summary>
    /// <param name="id">The ID of the packet (used for matching requests to responses)</param>
    /// <param name="type">The type of packet</param>
    /// <param name="content">The content of the packet</param>
    public RconPacket(int id, RconPacketType type, string content)
    {
        Id = id;
        RawType = (int)type;
        Content = content;
    }

    /// <summary>
    /// Prints a human readable version of the packet
    /// </summary>
    /// <returns>The human readable string</returns>
    public override string ToString()
    {
        return $"RconPacket: {Id} - {Type}({RawType}) - \"{Content}\"";
    }
}
