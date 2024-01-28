namespace PalWorld.Networking;

/// <summary>
/// A collection of extension methods to send packets to the server
/// </summary>
public static class PacketExtensions
{
    /// <summary>
    /// Broadcast a message to all players.
    /// Spaces in the message will be replaced with underscores (palworld issue).
    /// </summary>
    /// <param name="client">The client to send on</param>
    /// <param name="message">The message to send</param>
    /// <returns>The response from the server</returns>
    public static Task<RconPacket> SendBroadcast(this IRconSender client, string message)
    {
        return client.Send($"broadcast {message.Replace(" ", "_")}");
    }

    /// <summary>
    /// Broadcast a message to all players.
    /// Spaces in the message will be replaced with underscores (palworld issue).
    /// </summary>
    /// <param name="client">The client to send on</param>
    /// <param name="message">The message to send</param>
    /// <param name="args">The arguments to format in the message</param>
    /// <returns>The response from the server</returns>
    public static Task<RconPacket> SendBroadcast(this IRconSender client, string message, params object[] args)
    {
        return SendBroadcast(client, string.Format(message, args).Replace(" ", "_"));
    }

    /// <summary>
    /// Requests the server information
    /// </summary>
    /// <param name="client">The client to send on</param>
    /// <returns>The response from the server</returns>
    public static Task<RconPacket> SendInfo(this IRconSender client) => client.Send("info");

    /// <summary>
    /// Requests a list of all online players
    /// </summary>
    /// <param name="client">The client to send on</param>
    /// <returns>The response from the server</returns>
    public static Task<RconPacket> SendShowPlayers(this IRconSender client) => client.Send("showplayers");

    /// <summary>
    /// Kicks a player from the server
    /// </summary>
    /// <param name="client">The client to send on</param>
    /// <param name="uid">The game UID for the client to kick</param>
    /// <returns>The response from the server</returns>
    public static Task<RconPacket> SendKickPlayer(this IRconSender client, string uid) => client.Send($"kickplayer {uid}");

    /// <summary>
    /// Kicks a player from the server
    /// </summary>
    /// <param name="client">The client to send on</param>
    /// <param name="uid">The game UID for the client to kick</param>
    /// <returns>The response from the server</returns>
    public static Task<RconPacket> SendKickPlayer(this IRconSender client, int uid) => SendKickPlayer(client, uid.ToString());

    /// <summary>
    /// Bans a player from the server
    /// </summary>
    /// <param name="client">The client to send on</param>
    /// <param name="uid">The game UID for the client to ban</param>
    /// <returns>The response from the server</returns>
    public static Task<RconPacket> SendBanPlayer(this IRconSender client, string uid) => client.Send($"banplayer {uid}");

    /// <summary>
    /// Bans a player from the server
    /// </summary>
    /// <param name="client">The client to send on</param>
    /// <param name="uid">The game UID for the client to ban</param>
    /// <returns>The response from the server</returns>
    public static Task<RconPacket> SendBanPlayer(this IRconSender client, int uid) => SendBanPlayer(client, uid.ToString());

    /// <summary>
    /// Requests that the server save the world
    /// </summary>
    /// <param name="client">The client to send on</param>
    /// <returns>The response from the server</returns>
    public static Task<RconPacket> SendSave(this IRconSender client) => client.Send("save");

    /// <summary>
    /// Requests that the server gracefully shutdown
    /// </summary>
    /// <param name="client">The client to send on</param>
    /// <param name="seconds">How long to wait before shutting down after sending the message</param>
    /// <param name="message">The message to the online players</param>
    /// <returns>The response from the server</returns>
    public static Task<RconPacket> SendShutdown(this IRconSender client, int seconds, string message = "")
    {
        return client.Send($"shutdown {seconds} {message.Replace("_", " ")}".Trim());
    }

    /// <summary>
    /// Requests an immediate shutdown of the server
    /// </summary>
    /// <param name="client">The client to send on</param>
    /// <returns>The response from the server</returns>
    public static Task<RconPacket> SendExit(this IRconSender client) => client.Send("doexit");
}
