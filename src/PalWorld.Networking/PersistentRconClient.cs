namespace PalWorld.Networking;

/// <summary>
/// A version of the <see cref="IRconClient"/> that will lazily connect and ensure client readiness
/// </summary>
public interface IPersistentRconClient : IRconSender
{
    /// <summary>
    /// Attempts to connect to the server and authenticate.
    /// This is mostly to check if the credentials work or not.
    /// </summary>
    /// <returns>Whether or not the client was able to connect and authenticate</returns>
    Task<bool> Preconnect();
}

/// <summary>
/// The default implementation of <see cref="IPersistentRconClient"/>
/// </summary>
/// <param name="host">The IP address of the server to connect to</param>
/// <param name="port">The port of the server to connect to</param>
/// <param name="password">The admin password for server</param>
/// <param name="timeoutSec">How long to wait before considering a command ignored</param>
/// <param name="maxRetries">How many times to attempt to connect before considering the server dead</param>
/// <param name="encoding">The encoding to use for the packet body content</param>
/// <param name="useBase64">Whether or not to encode & decode Base64 messages with the server</param>
public class PersistentRconClient(
    string host, 
    int port, 
    string password, 
    int timeoutSec = 20,
    int maxRetries = 3,
    Encoding? encoding = null,
    bool useBase64 = false) : IPersistentRconClient
{
    /// <summary>
    /// Triggered when an unhandled exception occurs
    /// </summary>
    public event ExceptionCarrier OnError = delegate { };

    /// <summary>
    /// Triggered when the client is connected
    /// </summary>
    public event VoidCarrier OnConnected = delegate { };

    /// <summary>
    /// Triggered when the client is disconnected
    /// </summary>
    public event VoidCarrier OnDisconnected = delegate { };

    /// <summary>
    /// The underlying RconClient
    /// </summary>
    private RconClient? _client;

    /// <summary>
    /// Whether or not the client is ready to send packets
    /// </summary>
    public bool Ready => _client?.Ready ?? false;

    /// <summary>
    /// Whether or not the client is encoding & decoding Base64 messages with the server.
    /// </summary>
    public bool UseBase64 => _client?.UseBase64 ?? false;

    /// <summary>
    /// Ensure the client is ready and then send a packet once it is.
    /// </summary>
    /// <param name="message">The message to send</param>
    /// <param name="type">The type of packet to send</param>
    /// <returns>The response from the server</returns>
    public Task<RconPacket> Send(string message, RconPacketType type = RconPacketType.ExecCommand)
    {
        return Send(new RconPacket(0, type, message));
    }

    /// <summary>
    /// Ensure the client is ready and then send a packet once it is
    /// </summary>
    /// <param name="packet">The packet to send</param>
    /// <returns>The response from the server</returns>
    public Task<RconPacket> Send(RconPacket packet) => InternalWrite(packet, 0);

    /// <summary>
    /// Attempts to connect to the server and authenticate.
    /// This is mostly to check if the credentials work or not.
    /// </summary>
    /// <returns>Whether or not the client was able to connect and authenticate</returns>
    public async Task<bool> Preconnect()
    {
        var (client, authenticated) = await EnsureClient();
        return client is not null && authenticated;
    }

    /// <summary>
    /// Ensures the client is ready, throws an error if it is not.
    /// Once the client is ready, it attempts to send the packet.
    /// If the send fails due to a disconnected client, it will attempt to reconnect and resend up to the Max Retry Count.
    /// </summary>
    /// <param name="packet">The packet to send</param>
    /// <param name="tryCount">The current try number</param>
    /// <returns>The response from the server</returns>
    /// <exception cref="RconNetworkException">Thrown if an error occurs or max retries are exceeded</exception>
    public async Task<RconPacket> InternalWrite(RconPacket packet, int tryCount)
    {
        var (client, authenticated) = await EnsureClient();
        if (client is null) throw new RconNetworkException(RNEType.ClientDisconnected);
        if (!authenticated) throw new RconNetworkException(RNEType.AuthenticationFailed);

        try
        {
            return await client.Send(packet);
        }
        catch (RconNetworkException ex)
        {
            if (ex.Type != RNEType.ClientDisconnected &&
                tryCount >= maxRetries)
                throw;

            OnError(ex, $"Error sending packet, retrying ({tryCount + 1}/{maxRetries})");
            return await InternalWrite(packet, tryCount + 1);
        }
        catch (Exception ex)
        {
            OnError(ex, "Error sending packet");
            throw;
        }
    }

    /// <summary>
    /// Ensures the current client is ready to send packets
    /// </summary>
    /// <returns>The client and authentication result</returns>
    public async Task<ClientResult> EnsureClient()
    {
        if (_client is not null && _client.Ready)
            return new(_client, true);

        _client?.Dispose();
        _client = new RconClient(host, port, password, timeoutSec, encoding, useBase64);
        _client.OnConnected += () => OnConnected();
        _client.OnDisconnected += () => OnDisconnected();
        _client.OnError += (ex, note) => OnError(ex, note);

        if (await _client.Connect())
            return new(_client, await _client.Authenticate());

        _client.Dispose();
        _client = null;
        return new(null, false);
    }

    /// <summary>
    /// Disconnect the client and dispose of it
    /// </summary>
    public void Dispose()
    {
        _client?.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Represents the result of the <see cref="EnsureClient"/> method
    /// </summary>
    /// <param name="Client">The rcon client or null if it failed to connect</param>
    /// <param name="Authenticated">Whether or not the authentication request was successful</param>
    public record class ClientResult(IRconClient? Client, bool Authenticated);
}
