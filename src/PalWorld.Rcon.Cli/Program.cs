using PalWorld.Networking;
using static PalWorld.Rcon.Cli.HelperMethods;

var host = Prompt("RCON Host (IP Only): ");
var port = PromptInt("RCON Port (25575): ", 25575);
var pass = Prompt("RCON Password: ");

Console.Clear();

var client = new RconClient(host, port, pass);
client.OnException += (ex, note) => Print($"Exception: {ex.Message} ({note})", ConsoleColor.Red);
client.OnDisconnected += () => Print("Disconnected from server.", ConsoleColor.Red);
client.OnConnected += () => Print("Connected to server.", ConsoleColor.Green);
client.OnPacketReceived += (p) =>
{
    if (p.Type == RconPacketType.ServerResponseValue && p.Id == -1)
    {
        Print("Authentication failed.", ConsoleColor.Red);
        return;
    }

    if (p.Type == RconPacketType.ServerResponseValue)
    {
        Print(p.Content);
        return;
    }

    Print($"Server Sent: {p}", ConsoleColor.Cyan);
};

Print("Connecting to server...", ConsoleColor.Cyan);
if (!await client.Connect())
{
    Print("Failed to connect to server.", ConsoleColor.Red);
    return;
}

Print("Connected to server.", ConsoleColor.Green);

while(true)
{
    try
    {
        Print($"[{DateTime.Now:HH:mm:ss}] ", ConsoleColor.DarkGray, false);
        Print($"{host}:{port}", ConsoleColor.DarkYellow, false);
        var cmd = Prompt(" $ ", "help", ConsoleColor.Cyan);

        if (cmd == "exit")
        {
            client.Dispose();
            break;
        }

        if (cmd == "help")
        {
            PrintHelpText();
            continue;
        }

        await client.Send(cmd);
    }
    catch (Exception ex)
    {
        Print($"Error Occurred: {ex.Message}", ConsoleColor.Red);
        client.Dispose();
        break;
    }
}