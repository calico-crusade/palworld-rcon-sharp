namespace PalWorld.Bot.Database.Services;

using Base;
using Models;

public interface IPlayerDbService : IOrmMap<RconPlayer>
{
    Task<RconPlayer[]> ByServer(long id);
}

internal class PlayerDbService(IOrmService orm) : Orm<RconPlayer>(orm), IPlayerDbService
{
    private static string? _byServer;

    public Task<RconPlayer[]> ByServer(long id)
    {
        _byServer ??= Map.Select(t => t.With(a => a.ServerId));
        return Get(_byServer, new { ServerId = id });
    }
}
