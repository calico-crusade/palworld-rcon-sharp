namespace PalWorld.Rcon.Cli;

public class ConsoleCommand
{
    public required string Command { get; set; }

    public required string HelpText { get; set; }

    public string[] Parameters { get; set; } = [];

    public required string Category { get; set; }

    public static ConsoleCommand Create(string category, string cmd, string help, params string[] parameters)
        => new()
        {
            Command = cmd,
            HelpText = help,
            Parameters = parameters,
            Category = category,
        };
}
