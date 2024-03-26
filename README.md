# PalWorld Networking
PalWorld implemention of RCON in C#

## About this Repo
This repository is a fork of the official PalWorld Networking library, with a new feature to encode/decode Base64 messages with the server. 

This resolves the RCON from breaking when a player joins with a multi-byte character in their name like a symbol, causing the RCON to no longer respond. See more details [here](https://tech.palworldgame.com/api/rcon/).


## Requirements
This implementation requires **Palguard** to be installed on your Palworld server and Base64 enabled. You can get a copy by visiting the official Palguard Discord from this [page](https://github.com/Ultimeit/palguard_anticheat).

### Enabling Base64 Encoding/Decoding in Palguard
To enable Base64 encoding/decoding, edit the palguard.json file and set the RCONbase64 to true. This will allow the server to encode/decode messages between the RCON and clients after a reboot or running the "__/reloadcfg__" command to apply right away.

```json
{
    "RCONbase64": true,
    "adminIPs": [],
    ...
```

## Usage
Define an RconClient with the host, port and admin password of your server:

```csharp
using PalWorld.Networking;

var host = "192.168.1.69";
var port = 25575;
var pass = "S0mep4ssw0rd$"; //This should be the "Admin Password" not the server password

//Create your client with base64 mode enabled. (PersistentRconClient's will attempt to reconnect if they get disconnected!)
IPersistentRconClient client = new PersistentRconClient(host, port, pass, useBase64: true);

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
Open up an issue on this repo, or you can contact the official creator's [discord server](https://discord.gg/6H2eQAzcEj)