namespace PalWorld.Networking.Models;

/// <summary>
/// Represents the server's information.
/// </summary>
public class ServerInfo
{
    /// <summary>
    /// The name of the server
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The version of the server
    /// </summary>
    public string Version { get; set; } = string.Empty;
}
