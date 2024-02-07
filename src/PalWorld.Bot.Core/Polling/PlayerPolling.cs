namespace PalWorld.Bot.Core.Polling;

public interface IPlayerPolling
{
    RconPlayer[] OnlinePlayers { get; }

    Task Poll();
}

internal class PlayerPolling(
    IDbService _db,
    RconServer _server,
    RconPollInstance _instance,
    DiscordSocketClient _client) : IPlayerPolling
{
    private RconPlayer[]? _online;

    public RconPlayer[] OnlinePlayers => _online ?? [];
    public long ServerId => _instance.Id;

    public async Task Poll()
    {
        var online = await GetOnline();
        var isFirst = DetermineIsNew(online, out var wentOnline, out var wentOffline);
        _server.LastPlayersPoll = DateTime.UtcNow;

        if (isFirst || wentOffline.Length + wentOnline.Length == 0) return;

        await foreach (var guild in GetGuilds())
        {
            if (!guild.Enabled || guild.Channel is null) continue;

            var embeds = new[] 
            { 
                OnlineEmbed(wentOnline), 
                OfflineEmbed(wentOffline) 
            }.Where(t => t is not null)
                .Select(t => t!)
                .ToArray();

            await guild.Channel.SendMessageAsync(embeds: embeds);
        }
    }

    public string ServerName()
    {
        return _server.ServerName is null 
            ? "the server" 
            : $"{_server.ServerName} [{_server.ServerVersion}]";
    }

    public Embed? OnlineEmbed(RconPlayer[] players)
    {
        if (players is null || players.Length == 0) return null;

        var embed = new EmbedBuilder()
            .WithTitle("PalWorld Player Notification")
            .WithDescription($"{players.Length} player(s) joined {ServerName()}!")
            .WithColor(Color.Green)
            .WithCurrentTimestamp();

        foreach (var player in players)
            embed.AddField(player.Username, player.PlayerId, true);

        return embed.Build();
    }

    public Embed? OfflineEmbed(RconPlayer[] players)
    {
        if (players is null || players.Length == 0) return null;

        var embed = new EmbedBuilder()
            .WithTitle("PalWorld Player Notification")
            .WithDescription($"{players.Length} player(s) left {ServerName()}!")
            .WithColor(Color.Red)
            .WithCurrentTimestamp();

        foreach (var player in players)
            embed.AddField(player.Username, player.PlayerId, true);

        return embed.Build();
    }

    public async IAsyncEnumerable<PollGuild> GetGuilds()
    {
        var guilds = await _db.Guild.ByServer(_server.Id);
        foreach (var guild in guilds)
            yield return new PollGuild(guild, _client);
    }

    public async Task<RconPlayer[]> GetOnline()
    {
        var known = (await _db.Player.ByServer(ServerId)).ToList();
        var online = await _instance.Sender.GetPlayers();

        return await online.Select(async player =>
        {
            var existing = known.FirstOrDefault(t => t.PlayerId == player.Id)
            ?? new RconPlayer
            {
                ServerId = ServerId,
                PlayerId = player.Id,
                SteamId = player.SteamId,
                Username = player.Name,
                LastSeen = DateTime.UtcNow
            };
            existing.LastSeen = DateTime.UtcNow;
            existing.Id = await _db.Player.Upsert(existing);
            return existing;
        }).WhenAll();
    }

    public bool DetermineIsNew(RconPlayer[] latest, out RconPlayer[] online, out RconPlayer[] offline)
    {
        if (_online is null)
        {
            online = [.. latest];
            offline = [];
            _online = [.. latest];
            return true;
        }

        var wentOnline = new List<RconPlayer>();
        var wentOffline = new List<RconPlayer>();
        var all = latest
            .Concat(_online)
            .DistinctBy(t => t.PlayerId);

        foreach (var player in all)
        {
            var isOnline = latest.Any(t => t.PlayerId == player.PlayerId);
            var wasOnline = _online.Any(t => t.PlayerId == player.PlayerId);

            if (isOnline && !wasOnline)
                wentOnline.Add(player);
            if (!isOnline && wasOnline)
                wentOffline.Add(player);
        }

        online = [.. wentOnline];
        offline = [.. wentOffline];
        _online = [.. latest];
        return false;
    }
}
