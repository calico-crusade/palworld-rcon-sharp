namespace PalWorld.Bot.Database.Models;

[Table("rcon_guild")]
public class RconGuild : DbObject
{
    public long ServerId { get; set; }

    [Column(Unique = true)]
    public string GuildId { get; set; } = string.Empty;

    public string? ChannelId { get; set; }

    public bool PollEnabled { get; set; } = true;

    public string? Info { get; set; }
}
