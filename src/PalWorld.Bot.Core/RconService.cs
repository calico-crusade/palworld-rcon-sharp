namespace PalWorld.Bot.Core;

using Polling;

public interface IRconService
{
    Task StartAll();

    void StopAll();
}

internal class RconService(
    IDbService _db, 
    DiscordSocketClient _client) : IRconService
{
    private readonly ConcurrentDictionary<long, IRconPollInstance> _servers = [];

    public async Task StartAll()
    {
        var servers = await _db.Server.Get();
        foreach (var server in servers)
        {
            if (!_servers.ContainsKey(server.Id))
                _servers.TryAdd(server.Id, new RconPollInstance(_db, server, _client));

            _servers[server.Id].Start();
        }
    }

    public void StopAll()
    {
        foreach (var server in _servers.Values)
            server.Stop();
    }

    public async Task Refresh(long serverId)
    {
        if (_servers.TryRemove(serverId, out var old))
            old.Stop();

        var server = await _db.Server.Fetch(serverId);
        if (server is null) return;

        _servers.TryAdd(server.Id, new RconPollInstance(_db, server, _client));
        _servers[server.Id].Start();
    }

    public RconPlayer[] OnlinePlayers(long serverId)
    {
        return _servers.TryGetValue(serverId, out var value)
            ? value.OnlinePlayers
            : [];
    }

}
