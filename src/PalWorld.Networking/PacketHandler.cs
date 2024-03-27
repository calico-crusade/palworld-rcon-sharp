namespace PalWorld.Networking;

/// <summary>
/// Service for serializing and deserializing RCON packets
/// </summary>
public interface IPacketHandler
{
    /// <summary>
    /// Triggered when a full packet has been received
    /// </summary>
    event PacketCarrier OnPacketReceived;

    /// <summary>
    /// Serialize the given RCON packet into a byte array
    /// </summary>
    /// <param name="packet">The packet to serialize</param>
    /// <returns>The serialized packet data</returns>
    byte[] Serialize(RconPacket packet);

    /// <summary>
    /// Attempt to read the recon packet.
    /// </summary>
    /// <param name="data">The data to read</param>
    void Received(byte[] data);
}

internal class PacketHandler(IEncoderService _encoding) : IPacketHandler
{
    private static readonly IEncoderService AuthEncoder = PalEncoders.Default;

    private const int _indicatorSize = sizeof(int);
    private const int _maxPacketSize = 4096;

    public event PacketCarrier OnPacketReceived = delegate { };

    private byte[] _buffer = [];

    public byte[] Serialize(RconPacket packet)
    {
        var indicators = BitConverter.GetBytes(packet.Id)
            .Concat(BitConverter.GetBytes((int)packet.Type));

        var encoder = packet.Type == RconPacketType.Authentication 
            ? AuthEncoder
            : _encoding;

        var data = indicators.Concat(encoder.GetBytes(packet.Content + '\0'))
            .ToArray();

        if (data.Length > _maxPacketSize)
            throw new RconNetworkException(RNEType.PacketTooLarge);

        var length = data.Length + 1;
        var output = new byte[length + _indicatorSize];
        //Set last byte to null terminator
        output[length - 1] = 0;

        int i = 0;
        //Add length indicator (first 4 bytes)
        for (; i < 4; i++)
            output[i] = (byte)(length >> 8 * i);

        Buffer.BlockCopy(data, 0, output, i, data.Length);
        return output;
    }

    #region Deserializing
    public void Received(byte[] data)
    {
        foreach (var packet in Read(data))
            OnPacketReceived(Deserialize(packet));
    }

    public IEnumerable<byte[]> Read(byte[]? input = null)
    {
        if (input is not null && input.Length > 0)
            _buffer = [.. _buffer, .. input];

        if (_buffer.Length < _indicatorSize) yield break;

        int size = BitConverter.ToInt32(_buffer, 0);
        int actLength = size + _indicatorSize;
        if (_buffer.Length < actLength) yield break;

        var data = new byte[size];
        Array.Copy(_buffer, _indicatorSize, data, 0, size);
        _buffer = _buffer[actLength..];
        yield return data;

        foreach (var item in Read())
            yield return item;
    }

    public RconPacket Deserialize(byte[] data)
    {
        int i = 0;
        var id = BitConverter.ToInt32(data, i);
        i += _indicatorSize;
        var type = BitConverter.ToInt32(data, i);
        i += _indicatorSize;
        var body = _encoding.GetString(data, i, data.Length - i - 2);
        return new RconPacket(id, type, body);
    }
    #endregion
}
