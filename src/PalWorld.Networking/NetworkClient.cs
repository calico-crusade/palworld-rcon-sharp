namespace PalWorld.Networking;

/// <summary>
/// An instance of a TCP Network Client
/// </summary>
public interface INetworkClient
{
    /// <summary>
    /// When an exception occurs within the inner workings 
    /// (So we don't get any interuptions and can just log)
    /// </summary>
    event ExceptionCarrier OnException;

    /// <summary>
    /// When a client disconnects from the system
    /// </summary>
    event NetworkClientCarrier OnDisconnected;

    /// <summary>
    /// When data is received by the client
    /// </summary>
    event NetworkDataCarrier OnDataReceived;

    /// <summary>
    /// Is the TCP connection connected?
    /// </summary>
    bool Connected { get; }

    /// <summary>
    /// The other side's IP Addres
    /// </summary>
    string IPAddress { get; }

    /// <summary>
    /// Initiates the connection to the listener.
    /// </summary>
    /// <returns>Whether the connection was a success</returns>
    Task<bool> Start();

    /// <summary>
    /// Stop the connection
    /// </summary>
    void Stop();

    /// <summary>
    /// Write data to the connection's stream
    /// </summary>
    /// <param name="data">The data to write</param>
    /// <returns>Whether the data was written correctly</returns>
    Task<bool> Write(byte[] data);
}

internal class NetworkClient(string _host, int _port) : INetworkClient
{
    private TcpClient? _client;

    public bool Connected => _client?.Connected ?? false;
    public string IPAddress => $"{_host}:{_port}";
    public NetworkStream Stream => _client?.GetStream() 
        ?? throw new RconNetworkException(RNEType.NetworkStreamNotAvailable);

    public event ExceptionCarrier OnException = delegate { };
    public event NetworkClientCarrier OnDisconnected = delegate { };
    public event NetworkDataCarrier OnDataReceived = delegate { };

    public async Task<bool> Start()
    {
        //      ,_      _,
        //        '.__.'
        //   '-,   (__)   ,-'
        //     '._ .::. _.'
        //       _'(^^)'_
        //    _,` `>\/<` `,_
        //   `  ,-` )( `-,  `
        //      |  /==\  |
        //    ,-'  |=-|  '-,
        //         )-=(
        //         \__/
        // HELP! THERES A BUG IN MY CODE!
        try
        {
            _client = new TcpClient();
            await _client.ConnectAsync(_host, _port);
            ReadPacket();
            return Connected;
        }
        catch (SocketException)
        {
            return false;
        }
        catch (Exception ex)
        {
            OnException(ex, "Error Starting Client");
            return false;
        }
    }

    public void Stop()
    {
        try
        {
            Stream.Dispose();
            _client?.Dispose();
        }
        catch { }

        OnDisconnected(this);
    }

    public async Task<bool> Write(byte[] packet)
    {
        try
        {
            if (!Connected)
            {
                Stop();
                return false;
            }

            await Stream.WriteAsync(packet);

            return true;
        }
        catch (Exception ex)
        {
            OnException(ex, "Error writing data");
            return false;
        }
    }

    private async void ReadPacket()
    {
        try
        {
            if (!Connected)
            {
                Stop();
                return;
            }

            var buffer = new byte[_client!.ReceiveBufferSize];
            int length;
            if ((length = await Stream.ReadAsync(buffer)) == 0)
            {
                Stop();
                return;
            }

            if (length == buffer.Length)
            {
                OnDataReceived(this, buffer);
                ReadPacket();
                return;
            }

            var data = new byte[length];
            Array.Copy(buffer, data, length);

            OnDataReceived(this, data);
        }
        catch (IOException ex)
        {
            Stop();
            OnException(ex, "Network IO error");
            return;
        }
        catch (Exception ex)
        {
            OnException(ex, "Error processing packet");
        }

        ReadPacket();
    }

    public static NetworkClient Create(TcpClient client)
    {
        var ip = client.Client.RemoteEndPoint as IPEndPoint;
        var host = ip?.Address?.ToString() ?? throw new RconNetworkException(RNEType.NetworkAddressInvalid);
        var port = ip.Port;

        return new NetworkClient(host, port)
        {
            _client = client
        };
    }
}
