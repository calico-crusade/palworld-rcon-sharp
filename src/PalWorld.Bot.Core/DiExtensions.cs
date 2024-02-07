namespace PalWorld.Bot.Core;

public static class DiExtensions
{
    public static IServiceCollection AddPalWorldCore(this IServiceCollection services)
    {
        return services
            .AddSingleton<IRconService, RconService>();
    }
}
