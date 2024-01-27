namespace PalWorld.Networking;

public class RconNetworkException : Exception
{
    public RNEType Type { get; set; }

    public RconNetworkException(
        RNEType type, 
        string? message = null) : base(DetermineMessage(type, message))
    {
        Type = type;
    }

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
            _ => "An unknown error occurred",
        };
    }
}

public enum RNEType
{
    ClientDisconnected,
    FailedToSendPacket,
    NetworkAddressInvalid,
    NetworkStreamNotAvailable,
    PacketTooLarge,
}