namespace PalWorld.Bot.Database.Base;

using Models;

/// <summary>
/// Represents a cacheable ORM
/// </summary>
/// <typeparam name="T">The type of ORM</typeparam>
public abstract class CacheOrm<T> : Orm<T> where T : DbObject
{
    private static double _cacheTime = 5;
    private static CacheItem<T[]>? _cache;

    /// <summary>
    /// How long to wait before clearing the cache
    /// </summary>
    public virtual double CacheTime
    {
        get => _cache?.ExpireMinutes ?? _cacheTime;
        set
        {
            _cacheTime = value;
            ClearCache();
        }
    }

    /// <summary>
    /// Default CTOR
    /// </summary>
    /// <param name="orm">The ORM service</param>
    protected CacheOrm(IOrmService orm) : base(orm) { }

    /// <summary>
    /// Clears the cached values
    /// </summary>
    public void ClearCache() => _cache = null;

    /// <summary>
    /// Gets the values from the cache or database
    /// </summary>
    /// <returns>The values</returns>
    public override async Task<T[]> Get()
    {
        _cache ??= new CacheItem<T[]>(base.Get, CacheTime);
        return await _cache.Get() ?? [];
    }

    /// <summary>
    /// Overrides the upsert operation to clear the cache
    /// </summary>
    /// <param name="item">The item being upserted</param>
    /// <returns>The ID of the item being upserted</returns>
    public override Task<long> Upsert(T item)
    {
        ClearCache();
        return base.Upsert(item);
    }

    /// <summary>
    /// Overrides the delete operation to clear the cache
    /// </summary>
    /// <param name="id">The ID of the record being deleted</param>
    /// <returns>The number of rows deleted</returns>
    public override Task<int> Delete(long id)
    {
        ClearCache();
        return base.Delete(id);
    }

    /// <summary>
    /// Overrides the insert operation to clear the cache
    /// </summary>
    /// <param name="item">The item being inserted</param>
    /// <returns>The ID of the item being inserted</returns>
    public override Task<long> Insert(T item)
    {
        ClearCache();
        return base.Insert(item);
    }

    /// <summary>
    /// Overrides the update operation to clear the cache
    /// </summary>
    /// <param name="item">The item being updated</param>
    /// <returns>The number of records updated</returns>
    public override Task<int> Update(T item)
    {
        ClearCache();
        return base.Update(item);
    }
}
