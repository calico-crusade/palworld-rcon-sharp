namespace PalWorld.Bot.Database.Models;

[Table("rcon_server")]
public class RconServer : DbObject
{
    public string OwnerId { get; set; } = string.Empty;

    [Column(Unique = true)]
    public string ServerHost { get; set; } = string.Empty;

    [Column(Unique = true)]
    public int ServerPort { get; set; }

    public string Password { get; set; } = string.Empty;

    public int TimeoutSeconds { get; set; } = 20;

    public int MaxRetries { get; set; } = 3;

    public string? ServerName { get; set; }

    public string? ServerVersion { get; set; }

    public string[] AdminIds { get; set; } = [];

    public bool PollEnabled { get; set; }

    public int PollSeconds { get; set; } = 30;

    public DateTime? LastPlayersPoll { get; set; }

    public DateTime? LastInfoPoll { get; set; }
}
