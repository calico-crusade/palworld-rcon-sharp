# PalWorld Networking
PalWorld implemention of RCON in C#

## Installation
You can install [this package](https://www.nuget.org/packages/PalWorld.Networking) with any Nuget package manager:
```
PM> Install-Package PalWorld.Networking
```
It targets .net standard 2.1 from version 1.0.2+ so you can use it in .net framework or .net 5+

## Usage
Define an RconClient with the host, port and admin password of your server:

```csharp
using PalWorld.Networking;

var host = "192.168.1.69";
var port = 25575;
var pass = "S0mep4ssw0rd$"; //This should be the "Admin Password" not the server password

//Create your client (PersistentRconClient's will attempt to reconnect if they get disconnected!)
IPersistentRconClient client = new PersistentRconClient(host, port, pass);

//These aren't necessary, they are just helpful for logging!
client.OnConnected += () => Console.WriteLine("Hello world! I'm connected!");
client.OnDisconnected += () => Console.WriteLine("Goodbye world! I'm disconnected!");
client.OnError += (error, note) => Console.WriteLine($"Sorry! Something went wrong: {note} >> {error}");

//Check to make sure the presistent client can connect!
if (!await client.Preconnect())
{
    Console.WriteLine("I couldn't connect or login to the server!");
    return;
}

//Here are some default command handlers that I parse the responses of
var onlinePlayers = await client.Packets().GetPlayers();
foreach(var player in onlinePlayers)
{
    Console.WriteLine($"I see {player.Name} is online!");
}

var info = await client.Packets().GetInfo();
Console.WriteLine($"Server {info?.Name} is on version {info?.Version}");

//You can also send other commands like:
await client.SendBroadcast("Hello world!"); //Spaces will be substituted with underscores (palworld issue)
```

## Questions? Comments? Concerns?
Open up an issue on this repo, or you can contact me via my [discord server](https://discord.gg/6H2eQAzcEj)