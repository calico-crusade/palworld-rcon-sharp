namespace PalWorld.Networking;

/// <summary>
/// Represents a RCON client implementation that can send packets to a server
/// </summary>
public interface IRconSender : IDisposable
{
    /// <summary>
    /// Triggered when an unhandled exception occurs
    /// </summary>
    event ExceptionCarrier OnError;

    /// <summary>
    /// Triggered when the client is connected
    /// </summary>
    event VoidCarrier OnConnected;

    /// <summary>
    /// Triggered when the client is disconnected
    /// </summary>
    event VoidCarrier OnDisconnected;

    /// <summary>
    /// Whether or not the client is ready to send packets
    /// </summary>
    bool Ready { get; }

    /// <summary>
    /// Ensure the client is ready and then send a packet once it is.
    /// </summary>
    /// <param name="message">The message to send</param>
    /// <param name="type">The type of packet to send</param>
    /// <returns>The response from the server</returns>
    Task<RconPacket> Send(string message, RconPacketType type = RconPacketType.ExecCommand);

    /// <summary>
    /// Ensure the client is ready and then send a packet once it is
    /// </summary>
    /// <param name="packet">The packet to send</param>
    /// <returns>The response from the server</returns>
    Task<RconPacket> Send(RconPacket packet);
}
