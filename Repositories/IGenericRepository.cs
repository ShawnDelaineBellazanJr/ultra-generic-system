using System.Linq.Expressions;
using UltraGenericSystem.Models;

namespace UltraGenericSystem.Repositories;

/// <summary>
/// Generic repository interface for all entities in the ultra-generic system
/// </summary>
/// <typeparam name="T">The entity type that inherits from BaseEntity</typeparam>
public interface IGenericRepository<T> where T : BaseEntity
{
    /// <summary>
    /// Gets an entity by its ID
    /// </summary>
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all non-deleted entities
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new entity
    /// </summary>
    Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity
    /// </summary>
    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft deletes an entity by ID
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Hard deletes an entity by ID
    /// </summary>
    Task HardDeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries entities using a predicate
    /// </summary>
    Task<IEnumerable<T>> QueryAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets entities with pagination
    /// </summary>
    Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, 
        int pageSize, 
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts all non-deleted entities
    /// </summary>
    Task<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts entities matching a predicate
    /// </summary>
    Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an entity exists by ID
    /// </summary>
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if any entity matches a predicate
    /// </summary>
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets entities by tag
    /// </summary>
    Task<IEnumerable<T>> GetByTagAsync(string tag, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets entities by multiple tags
    /// </summary>
    Task<IEnumerable<T>> GetByTagsAsync(string[] tags, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets entities created by a specific user
    /// </summary>
    Task<IEnumerable<T>> GetByCreatedByAsync(string createdBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets entities created within a date range
    /// </summary>
    Task<IEnumerable<T>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets entities updated within a date range
    /// </summary>
    Task<IEnumerable<T>> GetByUpdatedDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets entities with metadata containing a specific key-value pair
    /// </summary>
    Task<IEnumerable<T>> GetByMetadataAsync(string key, object value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Restores a soft-deleted entity
    /// </summary>
    Task RestoreAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all soft-deleted entities
    /// </summary>
    Task<IEnumerable<T>> GetDeletedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the queryable for advanced operations
    /// </summary>
    IQueryable<T> GetQueryable();
} 