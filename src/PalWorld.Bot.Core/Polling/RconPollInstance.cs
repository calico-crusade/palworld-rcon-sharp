namespace PalWorld.Bot.Core.Polling;

public interface IRconPollInstance
{
    /// <summary>
    /// All of the players currently online
    /// </summary>
    RconPlayer[] OnlinePlayers { get; }

    /// <summary>
    /// Whether or not the polling is active
    /// </summary>
    bool PollingActive { get; }

    /// <summary>
    /// Starts polling (can be lazily started)
    /// </summary>
    void Start();

    /// <summary>
    /// Stops polling
    /// </summary>
    void Stop();

    /// <summary>
    /// Checks whether or not the rcon instance is valid
    /// </summary>
    /// <returns>Whether or not the rcon instance is valid</returns>
    Task<bool> IsValid();
}

public class RconPollInstance(
    IDbService _db,
    RconServer _server,
    DiscordSocketClient _client) : IRconPollInstance
{
    private PersistentRconClient? _recon;
    private CancellationTokenSource? _pollingToken;
    private DateTime? _pollingStarted;
    private PlayerPolling? _player;

    public CancellationToken PollingToken => _pollingToken?.Token ?? CancellationToken.None;
    public DateTime PollingStarted => _pollingStarted ?? DateTime.MinValue;
    public DateTime PollingLast => _server.LastPlayersPoll ?? DateTime.MinValue;
    public int PollTimeout => _server.PollSeconds * 1000;
    public bool PollingActive => _pollingToken is not null && !_pollingToken.IsCancellationRequested;
    public IPlayerPolling Player => _player ??= new(_db, _server, this, _client);
    public RconPlayer[] OnlinePlayers => Player.OnlinePlayers;

    public long Id => _server.Id;
    public IPersistentRconClient Rcon => _recon ??= new PersistentRconClient(
            _server.ServerHost,
            _server.ServerPort,
            _server.Password,
            _server.TimeoutSeconds,
            _server.MaxRetries);
    public IResponseParser Sender => Rcon.Packets();

    public Task<bool> IsValid() => Rcon.Preconnect();

    public void Stop()
    {
        _pollingToken?.Cancel();
        _pollingToken?.Dispose();
        _pollingToken = null;
    }

    public void Start()
    {
        if (PollingActive) return;

        _pollingToken = new();
        _ = Task.Run(Poll, PollingToken);
    }

    public async Task Poll()
    {
        try
        {
            _pollingStarted ??= DateTime.UtcNow;

            if (!PollingActive) return;

            await PollInfo();
            await Player.Poll();
            await Update();

            if (!PollingActive) return;

            await Task.Delay(PollTimeout, PollingToken);

            if (!PollingActive) return;

            await Poll();
        }
        catch
        {
            _pollingToken?.Cancel();
            _pollingToken?.Dispose();
            _pollingToken = null;
            throw;
        }
    }

    public async Task PollInfo()
    {
        var info = await Sender.GetInfo();
        if (info == null) return;

        _server.ServerVersion = info.Version;
        _server.ServerName = info.Name;
        _server.LastInfoPoll = DateTime.UtcNow;
    }

    public Task Update() => _db.Server.Update(_server);
}
