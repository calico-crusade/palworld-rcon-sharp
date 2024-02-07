using System.Linq.Expressions;

namespace PalWorld.Bot.Database.Base;

using Models;

/// <summary>
/// Represents a service that can query database models
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IOrmMap<T> where T : DbObject
{
    /// <summary>
    /// Fetches a single item from the database
    /// </summary>
    /// <param name="id">The ID of the item from the database</param>
    /// <returns>The item or null if not found</returns>
    Task<T?> Fetch(long id);

    /// <summary>
    /// Gets all of the items from the database
    /// </summary>
    /// <returns>The items in the database</returns>
    Task<T[]> Get();

    /// <summary>
    /// Inserts a new item into the database
    /// </summary>
    /// <param name="item">The item to insert into the database</param>
    /// <returns>The unique ID for the record that was inserted</returns>
    Task<long> Insert(T item);

    /// <summary>
    /// Updates the given item in the database by it's unique ID
    /// </summary>
    /// <param name="item">The item to update</param>
    /// <returns>The number of records updated</returns>
    Task<int> Update(T item);

    /// <summary>
    /// Deletes the given item in the database by it's unique ID
    /// </summary>
    /// <param name="id">The ID of the record to delete</param>
    /// <returns>The number of records deleted</returns>
    Task<int> Delete(long id);

    /// <summary>
    /// Gets a paginated list of items from the database, ordered by it's created date
    /// </summary>
    /// <param name="query">The query to execute</param>
    /// <param name="pars">The parameters to use during the execute</param>
    /// <param name="page">The page of records to get</param>
    /// <param name="size">The number of records per page</param>
    /// <returns>The paginated results</returns>
    Task<PaginatedResult<T>> Paginate(string query, object? pars = null, int page = 1, int size = 100);

    /// <summary>
    /// Gets a paginated list of items from the database, ordered by it's created date
    /// </summary>
    /// <param name="page">The page of records to get</param>
    /// <param name="size">The number of records per page</param>
    /// <returns>The paginated results</returns>
    Task<PaginatedResult<T>> Paginate(int page = 1, int size = 100);

    /// <summary>
    /// Inserts or updates the given item in the database by it's unique IDs
    /// </summary>
    /// <param name="item">The item to insert or update</param>
    /// <returns>The unique ID of the record that was inserted or updated</returns>
    Task<long> Upsert(T item);

    /// <summary>
    /// Gets the number of records in the current table
    /// </summary>
    /// <returns>The numerb of records in the table</returns>
    Task<int> Count();
}

/// <summary>
/// Represents a queryable object that can be used to generate SQL queries
/// </summary>
/// <typeparam name="T">The type of object to query against</typeparam>
public interface IOrmMapQueryable<T> : IOrmMap<T> where T : DbObject
{
    /// <summary>
    /// Generates a SQL query based on the given type and conditions
    /// </summary>
    /// <param name="where">The conditions to select using</param>
    /// <returns>The generated SQL query</returns>
    string Select(Action<IExpressionBuilder<T>> where);

    /// <summary>
    /// Generates an UPDATE SQL query for the given type
    /// </summary>
    /// <param name="where">The conditions to update against</param>
    /// <returns>The generated SQL UPDATE query</returns>
    string Update(Action<IExpressionBuilder<T>> where);

    /// <summary>
    /// Generates an UPDATE SQL query for the given type that only updates the given properties
    /// </summary>
    /// <param name="set">The columns that should be updated</param>
    /// <param name="where">The conditions to update against</param>
    /// <returns>The generated SQL UPDATE query</returns>
    string UpdateOnly(Action<IPropertyBinder<T>> set, Action<IExpressionBuilder<T>>? where = null);

    /// <summary>
    /// Generates a DELETE SQL query for the given type
    /// </summary>
    /// <param name="where">The conditions to delete against</param>
    /// <returns>The generated SQL DELETE query</returns>
    string Delete(Action<IExpressionBuilder<T>> where);

    /// <summary>
    /// Generates a paginated select query for the given type
    /// </summary>
    /// <typeparam name="TSort">The return type for the sort column</typeparam>
    /// <param name="sortBy">The property to sort the results by</param>
    /// <param name="sortAsc">Whether or not to sort the query in ascending (true) or descending (false) order</param>
    /// <param name="where">The conditions to query against</param>
    /// <param name="limitName">The name of the parameter to use for return row count limit</param>
    /// <param name="offsetName">The name of the parameter to use for the row offset</param>
    /// <returns>The generated SQL pagination query</returns>
    string Paginate<TSort>(Expression<Func<T, TSort>> sortBy, bool sortAsc = true, Action<IExpressionBuilder<T>>? where = null, string limitName = "limit", string offsetName = "offset");

    /// <summary>
    /// Generates an UPSERT SQL query for the given type
    /// </summary>
    /// <param name="conflicts">The columns that make up the composite unique key.</param>
    /// <returns>The generated SQL upsert query</returns>
    string Upsert(Action<IPropertyBinder<T>> conflicts);
}

/// <summary>
/// Implementation of the <see cref="IOrmMap{T}"/>
/// </summary>
/// <typeparam name="T">The type of object represented in the database</typeparam>
public class OrmMap<T> : IOrmMapQueryable<T> where T : DbObject
{
    #region Query Cache
    private static string? _fetchQuery;
    private static string? _insertQuery;
    private static string? _updateQuery;
    private static string? _deleteQuery;
    private static string? _paginateQuery;
    private static string? _getQuery;
    private static string? _upsertQuery;
    private static string? _countQuery;
    #endregion

    /// <summary>
    /// The service to generate queries
    /// </summary>
    public readonly IQueryService _query;
    /// <summary>
    /// The service that executes SQL queries
    /// </summary>
    public readonly ISqlService _sql;

    /// <summary>
    /// Implementation of the <see cref="IOrmMap{T}"/>
    /// </summary>
    /// <param name="query">The service to generate queries</param>
    /// <param name="sql">The service that executes SQL queries</param>
    public OrmMap(
        IQueryService query,
        ISqlService sql)
    {
        _query = query;
        _sql = sql;
    }

    #region Query Builders
    /// <summary>
    /// Generates a SQL query based on the given type and conditions
    /// </summary>
    /// <param name="where">The conditions to select using</param>
    /// <returns>The generated SQL query</returns>
    public string Select(Action<IExpressionBuilder<T>> where)
    {
        return _query.Select(where);
    }

    /// <summary>
    /// Generates an UPDATE SQL query for the given type
    /// </summary>
    /// <param name="where">The conditions to update against</param>
    /// <returns>The generated SQL UPDATE query</returns>
    public string Update(Action<IExpressionBuilder<T>> where)
    {
        return _query.Update(where);
    }

    /// <summary>
    /// Generates an UPDATE SQL query for the given type that only updates the given properties
    /// </summary>
    /// <param name="set">The columns that should be updated</param>
    /// <param name="where">The conditions to update against</param>
    /// <returns>The generated SQL UPDATE query</returns>
    public string UpdateOnly(Action<IPropertyBinder<T>> set, Action<IExpressionBuilder<T>>? where = null)
    {
        return _query.UpdateOnly(set, where);
    }

    /// <summary>
    /// Generates a DELETE SQL query for the given type
    /// </summary>
    /// <param name="where">The conditions to delete against</param>
    /// <returns>The generated SQL DELETE query</returns>
    public string Delete(Action<IExpressionBuilder<T>> where)
    {
        return _query.Delete(where);
    }

    /// <summary>
    /// Generates a paginated select query for the given type
    /// </summary>
    /// <typeparam name="TSort">The return type for the sort column</typeparam>
    /// <param name="sortBy">The property to sort the results by</param>
    /// <param name="sortAsc">Whether or not to sort the query in ascending (true) or descending (false) order</param>
    /// <param name="where">The conditions to query against</param>
    /// <param name="limitName">The name of the parameter to use for return row count limit</param>
    /// <param name="offsetName">The name of the parameter to use for the row offset</param>
    /// <returns>The generated SQL pagination query</returns>
    public string Paginate<TSort>(Expression<Func<T, TSort>> sortBy, bool sortAsc = true, Action<IExpressionBuilder<T>>? where = null, string limitName = "limit", string offsetName = "offset")
    {
        return _query.Paginate(sortBy, sortAsc, where, null, limitName, offsetName);
    }

    /// <summary>
    /// Generates an UPSERT SQL query for the given type
    /// </summary>
    /// <param name="conflicts">The columns that make up the composite unique key.</param>
    /// <returns>The generated SQL upsert query</returns>
    public string Upsert(Action<IPropertyBinder<T>> conflicts)
    {
        return _query.Upsert(conflicts);
    }
    #endregion

    #region Implementations
    /// <summary>
    /// Fetches a single item from the database
    /// </summary>
    /// <param name="id">The ID of the item from the database</param>
    /// <returns>The item or null if not found</returns>
    public virtual Task<T?> Fetch(long id)
    {
        _fetchQuery ??= _query.Select<T>(t =>
            t.Null(a => a.DeletedAt)
             .With(a => a.Id));
        return _sql.Fetch<T>(_fetchQuery, new { Id = id });
    }

    /// <summary>
    /// Gets all of the items from the database
    /// </summary>
    /// <returns>The items in the database</returns>
    public virtual Task<T[]> Get()
    {
        _getQuery ??= _query.Select<T>(t => t.Null(a => a.DeletedAt)) + " ORDER BY created_at ASC";
        return _sql.Get<T>(_getQuery);
    }

    /// <summary>
    /// Inserts a new item into the database
    /// </summary>
    /// <param name="item">The item to insert into the database</param>
    /// <returns>The unique ID for the record that was inserted</returns>
    public virtual Task<long> Insert(T item)
    {
        _insertQuery ??= _query.Insert<T>() + " RETURNING id";
        return _sql.ExecuteScalar<long>(_insertQuery, item);
    }

    /// <summary>
    /// Updates the given item in the database by it's unique ID
    /// </summary>
    /// <param name="item">The item to update</param>
    /// <returns>The number of records updated</returns>
    public virtual Task<int> Update(T item)
    {
        _updateQuery ??= _query.Update<T>();
        return _sql.Execute(_updateQuery, item);
    }

    /// <summary>
    /// Deletes the given item in the database by it's unique ID
    /// </summary>
    /// <param name="id">The ID of the record to delete</param>
    /// <returns>The number of records deleted</returns>
    public virtual Task<int> Delete(long id)
    {
        _deleteQuery ??= _query.Delete<T>();
        return _sql.Execute(_deleteQuery, new { Id = id });
    }

    /// <summary>
    /// Gets a paginated list of items from the database, ordered by it's created date
    /// </summary>
    /// <param name="query">The query to execute</param>
    /// <param name="pars">The parameters to use during the execute</param>
    /// <param name="page">The page of records to get</param>
    /// <param name="size">The number of records per page</param>
    /// <returns>The paginated results</returns>
    public virtual Task<PaginatedResult<T>> Paginate(string query, object? pars = null, int page = 1, int size = 100)
    {
        return _sql.Paginate<T>(query, pars, page, size);
    }

    /// <summary>
    /// Gets a paginated list of items from the database, ordered by it's created date
    /// </summary>
    /// <param name="page">The page of records to get</param>
    /// <param name="size">The number of records per page</param>
    /// <returns>The paginated results</returns>
    public virtual Task<PaginatedResult<T>> Paginate(int page = 1, int size = 100)
    {
        _paginateQuery ??= _query.Paginate<T, DateTime>(
            a => a.CreatedAt,
            true,
            t => t.Null(a => a.DeletedAt));
        return _sql.Paginate<T>(_paginateQuery, null, page, size);
    }

    /// <summary>
    /// Inserts or updates the given item in the database by it's unique IDs
    /// </summary>
    /// <param name="item">The item to insert or update</param>
    /// <returns>The unique ID of the record that was inserted or updated</returns>
    public virtual Task<long> Upsert(T item)
    {
        _upsertQuery ??= _query.Upsert<T>() + " RETURNING id";
        return _sql.ExecuteScalar<long>(_upsertQuery, item);
    }

    /// <summary>
    /// Gets the number of records in the current table
    /// </summary>
    /// <returns>The numerb of records in the table</returns>
    public virtual Task<int> Count()
    {
        if (string.IsNullOrWhiteSpace(_countQuery))
        {
            var type = _query.Type<T>();
            _countQuery = $"SELECT COUNT(*) FROM {type.Name.Name} WHERE deleted_at IS NULL";
        }

        return _sql.ExecuteScalar<int>(_countQuery);
    }
    #endregion
}
