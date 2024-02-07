namespace PalWorld.Bot.Database.Base;

/// <summary>
/// Represents a service that can hook into database operations
/// </summary>
public interface IDbInterjectService
{
    /// <summary>
    /// Hook into an ORM insert operation
    /// </summary>
    /// <typeparam name="T">The type being inserted</typeparam>
    /// <param name="entity">The object being inserted</param>
    /// <returns></returns>
    Task Insert<T>(T entity);

    /// <summary>
    /// Hook into an ORM update operation
    /// </summary>
    /// <typeparam name="T">The type being updated</typeparam>
    /// <param name="entity">The object being updated</param>
    /// <returns></returns>
    Task Update<T>(T entity);

    /// <summary>
    /// Hook into an ORM upsert operation
    /// </summary>
    /// <typeparam name="T">The type being upserted</typeparam>
    /// <param name="entity">The object being upserted</param>
    /// <returns></returns>
    Task Upsert<T>(T entity);

    /// <summary>
    /// Hook into an ORM delete operation
    /// </summary>
    /// <typeparam name="T">The type being deleted</typeparam>
    /// <param name="id">The ID of the object being deleted</param>
    /// <returns></returns>
    Task Delete<T>(long id);
}
