namespace PalWorld.Bot.Database.Base;

using Models;

/// <summary>
/// Wrap up service for ORMs
/// </summary>
public interface IOrmService
{
    /// <summary>
    /// The query service for the ORM
    /// </summary>
    IQueryService Query { get; }
    /// <summary>
    /// The SQL service for the ORM
    /// </summary>
    ISqlService Sql { get; }
    /// <summary>
    /// The optional interject service for the ORM
    /// </summary>
    IDbInterjectService? Interject { get; }

    /// <summary>
    /// The mapped queryable for the ORM
    /// </summary>
    /// <typeparam name="T">The type of table object</typeparam>
    /// <returns>The mapped queryable</returns>
    IOrmMapQueryable<T> For<T>() where T : DbObject;
}

internal class OrmService : IOrmService
{
    public IQueryService Query { get; }
    public ISqlService Sql { get; }
    public IDbInterjectService? Interject { get; }

    public OrmService(
        IQueryService query,
        ISqlService sql,
        IDbInterjectService? interject = null)
    {
        Query = query;
        Sql = sql;
        Interject = interject;
    }

    public IOrmMapQueryable<T> For<T>() where T : DbObject => new OrmMap<T>(Query, Sql);
}
