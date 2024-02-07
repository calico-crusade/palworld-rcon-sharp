namespace PalWorld.Bot.Database.Services;

using Base;
using Models;

public interface IServerDbService : IOrmMap<RconServer>
{

}

internal class ServerDbService(IOrmService orm) : Orm<RconServer>(orm), IServerDbService
{

}
