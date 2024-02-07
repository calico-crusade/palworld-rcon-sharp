namespace PalWorld.Bot.Database.Services;

using Base;
using Models;

public interface IGuildDbService : IOrmMap<RconGuild>
{
    Task<RconGuild[]> ByServer(long id);

    Task<RconGuild?> ByGuild(string id);
}

internal class GuildDbService(IOrmService _orm) : Orm<RconGuild>(_orm), IGuildDbService
{
    private static string? _byServer;
    private static string? _byGuild;

    public Task<RconGuild?> ByGuild(string id)
    {
        _byGuild ??= Map.Select(a => a.With(t => t.GuildId));
        return Fetch(_byGuild, new { GuildId = id });
    }

    public Task<RconGuild[]> ByServer(long id)
    {
        _byServer ??= Map.Select(a => a.With(t => t.ServerId));
        return Get(_byServer, new { ServerId = id });
    }
}
