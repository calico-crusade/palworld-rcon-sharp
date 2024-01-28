namespace PalWorld.Networking;

/// <summary>
/// Represents an exception that occurred during RCON communication
/// </summary>
/// <param name="type">The type of error that occurred</param>
/// <param name="message">An optional error message (inferred from type if none is passed)</param>
public class RconNetworkException(
    RNEType type,
    string? message = null) : Exception(DetermineMessage(type, message))
{
    /// <summary>
    /// The type of exception that occurred
    /// </summary>
    public RNEType Type { get; set; } = type;

    /// <summary>
    /// Determines the proper error message to use based on the type of exception
    /// </summary>
    /// <param name="type">The type of the exception</param>
    /// <param name="message">The overriding error message</param>
    /// <returns>The determined error message</returns>
    public static string DetermineMessage(RNEType type, string? message)
    {
        if (!string.IsNullOrEmpty(message)) return message;

        return type switch
        {
            RNEType.ClientDisconnected => "The client is not connected",
            RNEType.FailedToSendPacket => "Failed to send packet",
            RNEType.NetworkStreamNotAvailable => "Network Stream is not available on client",
            RNEType.NetworkAddressInvalid => "The network address is invalid",
            RNEType.PacketTooLarge => "The packet is too large to send",
            RNEType.AuthenticationFailed => "Authentication failed, RCON password is invalid",
            _ => "An unknown error occurred",
        };
    }
}

/// <summary>
/// The different types of the <see cref="RconNetworkException"/> that can occur
/// </summary>
public enum RNEType
{
    /// <summary>
    /// The client was either disconnected or has never been connected
    /// </summary>
    ClientDisconnected,
    /// <summary>
    /// The client failed to authenticate with the server
    /// </summary>
    AuthenticationFailed,
    /// <summary>
    /// Something happened during file send that caused the packet to fail to send
    /// </summary>
    FailedToSendPacket,
    /// <summary>
    /// The address passed to the client is invalid
    /// </summary>
    NetworkAddressInvalid,
    /// <summary>
    /// The TCP network stream was not available on request
    /// </summary>
    NetworkStreamNotAvailable,
    /// <summary>
    /// The network packet is too large to send (max size: 4096 bytes)
    /// </summary>
    PacketTooLarge,
}