namespace PalWorld.Networking;

/// <summary>
/// Handles network connection and packet communciation with the RCON server
/// </summary>
public interface IRconClient : IDisposable
{
    /// <summary>
    /// Triggered when a packet is received from the server
    /// </summary>
    event PacketCarrier OnPacketReceived;

    /// <summary>
    /// Triggered when an error occurred within the client
    /// </summary>
    event ExceptionCarrier OnException;

    /// <summary>
    /// Triggered when the client connects to the server
    /// </summary>
    event VoidCarrier OnConnected;

    /// <summary>
    /// Triggered when the client disconnects from the server
    /// </summary>
    event VoidCarrier OnDisconnected;

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
    /// Connect to the server and attempt login
    /// </summary>
    /// <returns>Whether or not the client was connected and authenticated</returns>
    Task<bool> Connect();

    /// <summary>
    /// Send a message to the server
    /// </summary>
    /// <param name="message">The message to send</param>
    /// <param name="type">The type of packet to send</param>
    /// <returns>The response from the server</returns>
    Task<RconPacket> Send(string message, RconPacketType type = RconPacketType.ExecCommand);

    /// <summary>
    /// Send a packet to the server
    /// </summary>
    /// <param name="packet">The packet to send</param>
    /// <returns>The response from the server</returns>
    Task<RconPacket> Send(RconPacket packet);
}

public class RconClient : IRconClient
{
    private static readonly Encoding _defaultEncoding = Encoding.UTF8;

    public event PacketCarrier OnPacketReceived = delegate { };
    public event ExceptionCarrier OnException = delegate { };
    public event VoidCarrier OnConnected = delegate { };
    public event VoidCarrier OnDisconnected = delegate { };
    public event GenericCarrier<byte[]> OnDataReceived = delegate { };

    private readonly Queue<TaskCompletionSource<RconPacket>> _queue;
    private readonly NetworkClient _client;
    private readonly PacketHandler _handler;
    private readonly string _password;
    private readonly int _cmdTimeoutSec;

    public INetworkClient Network => _client;
    public IPacketHandler Handler => _handler;
    public bool Connected => _client.Connected;

    public RconClient(string host, int port, string password, int cmdTimeoutSec = 20, Encoding? encoder = null)
    {
        _password = password;
        _cmdTimeoutSec = cmdTimeoutSec;
        _client = new NetworkClient(host, port);
        _client.OnDisconnected += (_) => Disconnected();
        _client.OnException += (e, n) => OnException(e, n);
        _client.OnDataReceived += (_, d) => ReadData(d);

        _handler = new PacketHandler(encoder ?? _defaultEncoding);
        _handler.OnPacketReceived += (p) => HandlePacket(p);

        _queue = new();
    }

    public async Task<bool> Connect()
    {
        if (_client.Connected) return true;

        ClearQueue();
        var connected = await _client.Start();
        if (!connected) return false;

        var resp = await Send(_password, RconPacketType.Authentication);
        return resp.Id > -1;
    }

    public Task<RconPacket> Send(string message, RconPacketType type = RconPacketType.ExecCommand)
    {
        var packet = new RconPacket(0, type, message);
        return Send(packet);
    }

    public async Task<RconPacket> Send(RconPacket packet)
    {
        if (!_client.Connected) 
            throw new RconNetworkException(RNEType.ClientDisconnected);

        var task = new TaskCompletionSource<RconPacket>();
        _queue.Enqueue(task);

        var data = _handler.Serialize(packet);
        if (!await _client.Write(data))
            throw new RconNetworkException(RNEType.FailedToSendPacket);

        return await task.Task.WaitAsync(TimeSpan.FromSeconds(_cmdTimeoutSec));
    }

    public void HandlePacket(RconPacket packet)
    {
        try
        {
            OnPacketReceived(packet);

            if (_queue.Count == 0) return;

            var task = _queue.Dequeue();
            task.SetResult(packet);
        }
        catch (Exception ex)
        {
            OnException(ex, $"Failed to handle packet: [{packet.Type} >> {packet.Id}] >> {packet.Content}");
        }
    }

    public void ReadData(byte[] data)
    {
        try
        {
            OnDataReceived(data);
            _handler.Received(data);
        }
        catch (Exception ex)
        {
            OnException(ex, "Failed to read packet data");
        }
    }

    public void ClearQueue()
    {
        while (_queue.Count > 0)
            _queue
                .Dequeue()
                .SetException(new RconNetworkException(RNEType.ClientDisconnected));
    }

    public void Disconnected()
    {
        OnDisconnected();
        ClearQueue();
    }

    public void Dispose()
    {
        _client.Stop();
        GC.SuppressFinalize(this);
    }
}
