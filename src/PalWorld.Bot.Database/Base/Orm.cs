namespace PalWorld.Bot.Database.Base;

using Models;

/// <summary>
/// Represents an ORM for database objects
/// </summary>
/// <typeparam name="T">The table type</typeparam>
/// <remarks>
/// The default CTOR
/// </remarks>
/// <param name="_orm">The ORM service</param>
public abstract class Orm<T>(IOrmService _orm) : IOrmMap<T> where T : DbObject
{
    /// <summary>
    /// The query service for the ORM
    /// </summary>
    public IQueryService Query => _orm.Query;
    /// <summary>
    /// The SQL service for the ORM
    /// </summary>
    public ISqlService Sql => _orm.Sql;

    /// <summary>
    /// Creates the connection
    /// </summary>
    public Task<IDbConnection> Con => Sql.CreateConnection();

    /// <summary>
    /// The mappable query service for the ORM
    /// </summary>
    public IOrmMapQueryable<T> Map => _orm.For<T>();

    /// <summary>
    /// Fetches a single record from the database
    /// </summary>
    /// <param name="query">The query to run to fetch the record</param>
    /// <param name="param">The parameters to run the query with</param>
    /// <returns>A single record or null from the database</returns>
    public virtual Task<T?> Fetch(string query, object? param = null) => Sql.Fetch<T>(query, param);

    /// <summary>
    /// Fetches mutliple records from the database
    /// </summary>
    /// <param name="query">The query to run to fetch the records</param>
    /// <param name="param">The parameters to run the query with</param>
    /// <returns>A multiple records or an empty array from the database</returns>
    public virtual Task<T[]> Get(string query, object? param = null) => Sql.Get<T>(query, param);

    /// <summary>
    /// Executes the given query and returns the record count
    /// </summary>
    /// <param name="query">The query to execute</param>
    /// <param name="param">The parameters to run the query with</param>
    /// <returns>The number of records that were modified by the query (or whatever the return code of the query was)</returns>
    public virtual Task<int> Execute(string query, object? param = null) => Sql.Execute(query, param);

    /// <summary>
    /// Executes the given query and returns the scalar result
    /// </summary>
    /// <typeparam name="T1">The type of scalar return result</typeparam>
    /// <param name="query">The query to execute</param>
    /// <param name="param">The parameters to run the query with</param>
    /// <returns>The return result of the query</returns>
    public virtual Task<T1> Execute<T1>(string query, object? param = null) => Sql.ExecuteScalar<T1>(query, param);

    /// <summary>
    /// Fetches a single item from the database
    /// </summary>
    /// <param name="id">The ID of the item from the database</param>
    /// <returns>The item or null if not found</returns>
    public virtual Task<T?> Fetch(long id) => Map.Fetch(id);

    /// <summary>
    /// Gets all of the items from the database
    /// </summary>
    /// <returns>The items in the database</returns>
    public virtual Task<T[]> Get() => Map.Get();

    /// <summary>
    /// Inserts a new item into the database
    /// </summary>
    /// <param name="item">The item to insert into the database</param>
    /// <returns>The unique ID for the record that was inserted</returns>
    public virtual async Task<long> Insert(T item)
    {
        item.Id = await Map.Insert(item);
        if (_orm.Interject is not null)
            await _orm.Interject.Insert(item);
        return item.Id;
    }

    /// <summary>
    /// Updates the given item in the database by it's unique ID
    /// </summary>
    /// <param name="item">The item to update</param>
    /// <returns>The number of records updated</returns>
    public virtual async Task<int> Update(T item)
    {
        var res = await Map.Update(item);
        if (_orm.Interject is not null)
            await _orm.Interject.Update(item);
        return res;
    }

    /// <summary>
    /// Deletes the given item in the database by it's unique ID
    /// </summary>
    /// <param name="id">The ID of the record to delete</param>
    /// <returns>The number of records deleted</returns>
    public virtual async Task<int> Delete(long id)
    {
        var res = await Map.Delete(id);
        if (_orm.Interject is not null)
            await _orm.Interject.Delete<T>(id);
        return res;
    }

    /// <summary>
    /// Inserts or updates the given item in the database by it's unique IDs
    /// </summary>
    /// <param name="item">The item to insert or update</param>
    /// <returns>The unique ID of the record that was inserted or updated</returns>
    public virtual async Task<long> Upsert(T item)
    {
        item.Id = await Map.Upsert(item);
        if (_orm.Interject is not null)
            await _orm.Interject.Upsert(item);
        return item.Id;
    }

    /// <summary>
    /// Gets the number of records in the current table
    /// </summary>
    /// <returns>The numerb of records in the table</returns>
    public virtual Task<int> Count() => Map.Count();

    /// <summary>
    /// Gets a paginated list of items from the database, ordered by it's created date
    /// </summary>
    /// <param name="page">The page of records to get</param>
    /// <param name="size">The number of records per page</param>
    /// <returns>The paginated results</returns>
    public virtual Task<PaginatedResult<T>> Paginate(int page = 1, int size = 100) => Map.Paginate(page, size);

    /// <summary>
    /// Gets a paginated list of items from the database, ordered by it's created date
    /// </summary>
    /// <param name="query">The query to execute</param>
    /// <param name="pars">The parameters to use during the execute</param>
    /// <param name="page">The page of records to get</param>
    /// <param name="size">The number of records per page</param>
    /// <returns>The paginated results</returns>
    public virtual Task<PaginatedResult<T>> Paginate(string query, object? pars = null, int page = 1, int size = 100) => Map.Paginate(query, pars, page, size);
}
