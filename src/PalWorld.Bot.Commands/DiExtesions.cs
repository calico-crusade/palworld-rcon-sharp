namespace PalWorld.Bot.Commands;

public static class DiExtensions
{
    public static IServiceCollection AddBotServices(this IServiceCollection services)
    {
        return services
            .AddTransient<IBackgroundHandler, BackgroundHandler>();
    }

    public static IDiscordSlashCommandBuilder AddBotCommands(this IDiscordSlashCommandBuilder bob)
    {
        return bob;
    }
}
