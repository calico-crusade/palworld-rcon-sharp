namespace PalWorld.Bot.Database;

using Services;

public interface IDbService
{
    IPlayerDbService Player { get; }
    IServerDbService Server { get; }
    IGuildDbService Guild { get; }
}

internal class DbService(
    IPlayerDbService player,
    IServerDbService server,
    IGuildDbService guild) : IDbService
{
    public IPlayerDbService Player { get; } = player;
    public IServerDbService Server { get; } = server;
    public IGuildDbService Guild { get; } = guild;
}
