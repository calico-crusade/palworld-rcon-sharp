namespace PalWorld.Bot.Commands;

using Core;

internal class BotCommands(
    IRconService _rcon,
    ILogger<BotCommands> _logger)
{

    [Command("hello", "Checks if the bot is working")]
    public Task Hello(SocketSlashCommand cmd) => cmd.RespondAsync("Hi", ephemeral: true);
}
