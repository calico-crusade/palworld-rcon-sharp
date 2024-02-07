namespace PalWorld.Bot.Database;

using Base;
using Models;
using Services;

public static class DiExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services)
    {
        return services
            .AddSqlService(c =>
            {
                c.ConfigureGeneration(a => a.WithCamelCaseChange())
                 .ConfigureTypes(a =>
                 {
                     var con = a.CamelCase()
                        .Entity<RconPlayer>()
                        .Entity<RconServer>()
                        .Entity<RconGuild>();

                     a.ArrayHandler<string>()
                      .PolyfillDateTimeHandler()
                      .PolyfillBooleanHandler();
                 });

                c.AddSQLite<SqliteConfig>(a =>
                {
                    a.OnInit(con => new DatabaseDeploy(con, "Scripts").ExecuteScripts());
                });
            })
            
            .AddTransient<IPlayerDbService, PlayerDbService>()
            .AddTransient<IServerDbService, ServerDbService>()
            .AddTransient<IGuildDbService, GuildDbService>()
            
            .AddTransient<IDbService, DbService>();
    }
}
