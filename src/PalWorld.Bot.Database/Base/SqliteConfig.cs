using Microsoft.Data.Sqlite;

namespace PalWorld.Bot.Database.Base;

internal class SqliteConfig(IConfiguration _config) : ISqlConfig<SqliteConnection>
{
    public string ConnectionString =>
        _config["Database:ConnectionString"]
            ?? throw new NullReferenceException("Database:ConnectionString - Required setting is not present");

    public int Timeout => int.TryParse(_config["Database:Timeout"], out int timeout) ? timeout : 0;
}
