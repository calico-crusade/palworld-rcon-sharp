namespace PalWorld.Networking;

/// <summary>
/// An error occurred within the network
/// </summary>
/// <param name="ex">The Exception that occurred</param>
/// <param name="note">Any note on where the exception occurred</param>
public delegate void ExceptionCarrier(Exception ex, string note);

/// <summary>
/// When data is received by the client
/// </summary>
/// <param name="client">The client that received the data</param>
/// <param name="data">The data that was received</param>
public delegate void NetworkDataCarrier(INetworkClient client, byte[] data);

/// <summary>
/// When a client connects or disconnects from the system
/// </summary>
/// <param name="client">The client that was connected or disconnected</param>
public delegate void NetworkClientCarrier(INetworkClient client);

/// <summary>
/// When a full packet is received by the client
/// </summary>
/// <param name="packet">The packet that was received</param>
public delegate void PacketCarrier(RconPacket packet);

/// <summary>
/// When something happens that doesn't need to return anything
/// </summary>
public delegate void VoidCarrier();

/// <summary>
/// When something happens that has 1 parameter
/// </summary>
/// <typeparam name="T">The type of data for the event</typeparam>
/// <param name="data">The data that was received</param>
public delegate void GenericCarrier<T>(T data);
