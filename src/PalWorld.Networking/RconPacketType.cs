namespace PalWorld.Networking;

/// <summary>
/// The different types of packets that can be sent
/// </summary>
public enum RconPacketType
{
    /// <summary>
    /// The server has responded to a request
    /// </summary>
    ServerResponseValue = 0,
    /// <summary>
    /// The client is attempting to execute a command
    /// </summary>
    ExecCommand = 2,
    /// <summary>
    /// The client is attempting to authenticate with the server
    /// </summary>
    Authentication = 3,
}