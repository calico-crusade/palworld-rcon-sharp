namespace PalWorld.Bot.Commands;

using Core;

public interface IBackgroundHandler
{
    Task StartAll();
}

internal class BackgroundHandler(IRconService _rcon) : IBackgroundHandler
{
    public Task StartAll() => _rcon.StartAll();
}
