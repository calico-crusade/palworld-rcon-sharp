namespace PalWorld.Bot.Database.Models;

[Table("rcon_player")]
public class RconPlayer : DbObject
{
    [JsonPropertyName("serverId"), Column(Unique = true)]
    public long ServerId { get; set; }

    [JsonPropertyName("playerId"), Column(Unique = true)]
    public string PlayerId { get; set; } = string.Empty;

    [JsonPropertyName("steamId")]
    public string SteamId { get; set; } = string.Empty;

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("discordId")]
    public string? DiscordId { get; set; }

    [JsonPropertyName("lastSeen")]
    public DateTime LastSeen { get; set; }
}
