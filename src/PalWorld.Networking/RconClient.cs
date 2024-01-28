namespace PalWorld.Networking;

/// <summary>
/// Handles network connection and packet communciation with the RCON server
/// </summary>
public interface IRconClient : IRconSender
{
    /// <summary>
    /// Triggered when a packet is received from the server
    /// </summary>
    event PacketCarrier OnPacketReceived;

    /// <summary>
    /// Triggered when data is received on the underlying network connection
    /// </summary>
    event GenericCarrier<byte[]> OnDataReceived;

    /// <summary>
    /// The underlying network connection
    /// </summary>
    INetworkClient Network { get; }

    /// <summary>
    /// The underlying packet handler
    /// </summary>
    IPacketHandler Handler { get; }

    /// <summary>
    /// Whether or not the client is connected to the server
    /// </summary>
    bool Connected { get; }

    /// <summary>
    /// Whether or not the client is authenticated with the server
    /// </summary>
    bool Authenticated { get; }

    /// <summary>
    /// Connect to the server
    /// </summary>
    /// <returns>Whether or not the client was connected</returns>
    Task<bool> Connect();

    /// <summary>
    /// Attempt to authenticate a connected client
    /// </summary>
    /// <returns>Whether or not the client was authenticated</returns>
    Task<bool> Authenticate();

    /// <summary>
    /// Short hand for Connect() and Authenticate()
    /// </summary>
    /// <returns></returns>
    Task<bool> ConnectAndAuthenticate();
}

/// <summary>
/// Handles network connection and packet communciation with the RCON server
/// </summary>
public class RconClient : IRconClient
{
    /// <summary>
    /// The default encoding to use for the packet body content
    /// </summary>
    private static readonly Encoding _defaultEncoding = Encoding.UTF8;

    #region Events
    /// <summary>
    /// Triggered when a packet is received from the server
    /// </summary>
    public event PacketCarrier OnPacketReceived = delegate { };

    /// <summary>
    /// Triggered when an error occurred within the client
    /// </summary>
    public event ExceptionCarrier OnError = delegate { };

    /// <summary>
    /// Triggered when the client connects to the server
    /// </summary>
    public event VoidCarrier OnConnected = delegate { };

    /// <summary>
    /// Triggered when the client disconnects from the server
    /// </summary>
    public event VoidCarrier OnDisconnected = delegate { };

    /// <summary>
    /// Triggered when data is received on the underlying network connection
    /// </summary>
    public event GenericCarrier<byte[]> OnDataReceived = delegate { };
    #endregion

    #region Fields
    /// <summary>
    /// Queue for handling packet responses from the server
    /// </summary>
    private readonly Queue<TaskCompletionSource<RconPacket>> _queue;

    /// <summary>
    /// The underlying network connection
    /// </summary>
    private readonly NetworkClient _client;

    /// <summary>
    /// The underlying packet handler
    /// </summary>
    private readonly PacketHandler _handler;

    /// <summary>
    /// The password for connecting to the RCON server
    /// </summary>
    private readonly string _password;

    /// <summary>
    /// How long to wait before considering a command ignored
    /// </summary>
    private readonly int _cmdTimeoutSec;
    #endregion

    #region Properties
    /// <summary>
    /// The underlying network connection
    /// </summary>
    public INetworkClient Network => _client;

    /// <summary>
    /// The underlying packet handler
    /// </summary>
    public IPacketHandler Handler => _handler;

    /// <summary>
    /// Whether or not the client is connected to the server
    /// </summary>
    public bool Connected => _client.Connected;

    /// <summary>
    /// Whether or not the client is authenticated with the server
    /// </summary>
    public bool Authenticated { get; private set; }

    /// <summary>
    /// Whether or not the client is connected and authenticated with the server
    /// </summary>
    public bool Ready => Connected && Authenticated;
    #endregion

    /// <summary>
    /// Create a new RCON client
    /// </summary>
    /// <param name="host">The IP Address of the server to connect to</param>
    /// <param name="port">The Port of the RCON server to connect to</param>
    /// <param name="password">The Admin password for the server</param>
    /// <param name="cmdTimeoutSec">How long to wait before considering a command ignored</param>
    /// <param name="encoder">The encoding to use for the packet body content</param>
    public RconClient(string host, int port, string password, int cmdTimeoutSec = 20, Encoding? encoder = null)
    {
        _password = password;
        _cmdTimeoutSec = cmdTimeoutSec;
        _client = new NetworkClient(host, port);
        _client.OnDisconnected += (_) => Disconnected();
        _client.OnException += (e, n) => OnError(e, n);
        _client.OnDataReceived += (_, d) => ReadData(d);

        _handler = new PacketHandler(encoder ?? _defaultEncoding);
        _handler.OnPacketReceived += HandlePacket;

        _queue = new();
    }

    #region Exposed Methods
    /// <summary>
    /// Connect to the server
    /// </summary>
    /// <returns>Whether or not the client was connected</returns>
    public async Task<bool> Connect()
    {
        if (_client.Connected) return true;

        ClearQueue();
        Authenticated = false;
        return await _client.Start();
    }

    /// <summary>
    /// Attempt to authenticate a connected client
    /// </summary>
    /// <returns>Whether or not the client was authenticated</returns>
    public async Task<bool> Authenticate()
    {
        if (Ready) return true;
        if (!Connected) return false;

        var resp = await Send(_password, RconPacketType.Authentication);
        return Authenticated = resp.Id > -1;
    }

    /// <summary>
    /// Short hand for Connect() and Authenticate()
    /// </summary>
    /// <returns></returns>
    public async Task<bool> ConnectAndAuthenticate()
    {
        return await Connect() && await Authenticate();
    }

    /// <summary>
    /// Send a message to the server
    /// </summary>
    /// <param name="message">The message to send</param>
    /// <param name="type">The type of packet to send</param>
    /// <returns>The response from the server</returns>
    public Task<RconPacket> Send(string message, RconPacketType type = RconPacketType.ExecCommand)
    {
        var packet = new RconPacket(0, type, message);
        return Send(packet);
    }

    /// <summary>
    /// Send a packet to the server
    /// </summary>
    /// <param name="packet">The packet to send</param>
    /// <returns>The response from the server</returns>
    public async Task<RconPacket> Send(RconPacket packet)
    {
        if (!Connected) throw new RconNetworkException(RNEType.ClientDisconnected);
        if (!Authenticated && packet.Type != RconPacketType.Authentication) 
            throw new RconNetworkException(RNEType.AuthenticationFailed);

        var task = new TaskCompletionSource<RconPacket>();
        _queue.Enqueue(task);

        var data = _handler.Serialize(packet);
        if (!await _client.Write(data))
            throw new RconNetworkException(RNEType.FailedToSendPacket);

        return await task.Task.WaitAsync(TimeSpan.FromSeconds(_cmdTimeoutSec));
    }
    #endregion

    #region Internal Methods
    /// <summary>
    /// Handler for the <see cref="IPacketHandler.OnPacketReceived"/> event
    /// </summary>
    /// <param name="packet">The packet that was received</param>
    public void HandlePacket(RconPacket packet)
    {
        try
        {
            OnPacketReceived(packet);

            if (_queue.Count == 0) return;

            _queue.Dequeue().SetResult(packet);
        }
        catch (Exception ex)
        {
            OnError(ex, $"Failed to handle packet: [{packet.Type} >> {packet.Id}] >> {packet.Content}");
        }
    }

    /// <summary>
    /// Handler for the <see cref="INetworkClient.OnDataReceived"/> event
    /// </summary>
    /// <param name="data">The data that was received</param>
    public void ReadData(byte[] data)
    {
        try
        {
            OnDataReceived(data);
            _handler.Received(data);
        }
        catch (Exception ex)
        {
            OnError(ex, "Failed to read packet data");
        }
    }

    /// <summary>
    /// Clears the queue of pending packet response tasks, failing them.
    /// </summary>
    public void ClearQueue()
    {
        while (_queue.Count > 0)
            _queue
                .Dequeue()
                .SetException(new RconNetworkException(RNEType.ClientDisconnected));
    }

    /// <summary>
    /// Handler for the <see cref="INetworkClient.OnDisconnected"/> event
    /// </summary>
    public void Disconnected()
    {
        Authenticated = false;
        OnDisconnected();
        ClearQueue();
    }

    /// <summary>
    /// Disconnects the network client, disposing of it and clearing the task queue
    /// </summary>
    public void Dispose()
    {
        ClearQueue();
        _client.Stop();
        GC.SuppressFinalize(this);
    }
    #endregion
}