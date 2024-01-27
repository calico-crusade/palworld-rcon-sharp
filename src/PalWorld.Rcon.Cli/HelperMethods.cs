namespace PalWorld.Rcon.Cli;

public static class HelperMethods
{
    public static void Print(string message, ConsoleColor? color = null, bool line = true)
    {
        var current = Console.ForegroundColor;
        if (color is not null)
            Console.ForegroundColor = color!.Value;
        if (line) Console.WriteLine(message);
        else Console.Write(message);
        Console.ForegroundColor = current;
    }

    public static string Prompt(string message, string? @default = null, ConsoleColor? color = null)
    {
        Print(message, color, false);
        var output = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(output) && @default is not null)
            return @default!;

        while (string.IsNullOrEmpty(output))
        {
            Print("Input invalid", ConsoleColor.Red);
            Print(message, color, false);
            output = Console.ReadLine();
        }

        return output!;
    }

    public static int PromptInt(string message, int? @default = null, ConsoleColor? color = null)
    {
        Print(message, color, false);
        var output = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(output) && @default is not null)
            return @default!.Value;

        while (!int.TryParse(output, out var _))
        {
            Print("Input invalid (number)", ConsoleColor.Red);
            Print(message, color, false);
            output = Console.ReadLine();
        }

        return int.Parse(output!);
    }

    public static ConsoleCommand[] Commands()
    {
        return
        [
            ConsoleCommand.Create("General", "broadcast", "Broadcast a message to all players (use _ not spaces)", "message"),
            ConsoleCommand.Create("General", "info", "Gets information about the server"),
            ConsoleCommand.Create("General", "showplayers", "Shows a list of all players currently on the server"),

            ConsoleCommand.Create("Player Administration", "kickplayer", "Kicks the player by their steam or game ID", "playerId"),
            ConsoleCommand.Create("Player Administration", "banplayer", "Bans the player by their steam or game ID", "playerId"),

            ConsoleCommand.Create("Administration", "save", "Triggers a server wide game save"),
            ConsoleCommand.Create("Administration", "shutdown", "Triggers a graceful server shutdown (use _ not spaces in the message)", "seconds", "message"),
            ConsoleCommand.Create("Administration", "doexit", "Force shutdown the server without a timer"),

            ConsoleCommand.Create("Internal", "help", "Shows this help menu"),
            ConsoleCommand.Create("Internal", "exit", "Disconnects from the server and exits the application"),
        ];
    }

    public static void PrintHelpText()
    {
        var commands = Commands();

        Print("PalWorld RCON Commands:", ConsoleColor.Cyan);
        Print("Tool for connecting to and executing server commands", ConsoleColor.DarkGray);
        Print("");

        foreach (var cmds in commands.GroupBy(t => t.Category))
        {
            Print($"{cmds.Key} Commands", ConsoleColor.White);

            foreach (var cmd in cmds)
            {
                Print($"\t{cmd.Command} ", ConsoleColor.Cyan, false);

                foreach (var param in cmd.Parameters)
                {
                    Print("{", ConsoleColor.White, false);
                    Print(param, ConsoleColor.Yellow, false);
                    Print("} ", ConsoleColor.White, false);
                }

                Print("- ", ConsoleColor.White, false);
                Print(cmd.HelpText, ConsoleColor.DarkGray);
            }

            Print("");
        }

        Print("Need help? https://github.com/calico-crusade/palworld-rcon-sharp");
    }
}
