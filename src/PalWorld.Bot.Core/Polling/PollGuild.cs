namespace PalWorld.Bot.Core.Polling;

public class PollGuild(
    RconGuild _guild,
    DiscordSocketClient _client)
{
    private ulong? _guildId;
    private ulong? _channelId;
    private SocketGuild? _discordGuild;
    private SocketTextChannel? _discordChannel;

    public bool Enabled => _guild.PollEnabled;
    public string? Info => _guild.Info;
    public ulong GuildId => _guildId ??= ulong.Parse(_guild.GuildId);
    public SocketGuild? Guild => _discordGuild ??= _client.GetGuild(GuildId);
    public ulong? ChannelId => _channelId ??= _guild.ChannelId is null ? null : ulong.Parse(_guild.ChannelId);
    public SocketTextChannel? Channel => _discordChannel ??= _guild.ChannelId is null ? null : Guild?.GetTextChannel(ChannelId!.Value);
}
