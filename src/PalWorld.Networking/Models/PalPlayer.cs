namespace PalWorld.Networking.Models;

/// <summary>
/// Represents a player in the game.
/// </summary>
public class PalPlayer
{
    /// <summary>
    /// The player's name in game.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The player's Steam ID
    /// </summary>
    public string SteamId { get; set; } = string.Empty;

    /// <summary>
    /// The player's Character ID
    /// </summary>
    public string Id { get; set; } = string.Empty;
}
