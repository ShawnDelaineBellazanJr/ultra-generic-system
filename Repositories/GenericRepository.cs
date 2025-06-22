using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UltraGenericSystem.Data;
using UltraGenericSystem.Models;

namespace UltraGenericSystem.Repositories;

/// <summary>
/// Generic repository implementation for all entities in the ultra-generic system
/// </summary>
/// <typeparam name="T">The entity type that inherits from BaseEntity</typeparam>
public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    private readonly UltraGenericContext _context;
    private readonly DbSet<T> _dbSet;
    private readonly ILogger<GenericRepository<T>> _logger;

    public GenericRepository(UltraGenericContext context, ILogger<GenericRepository<T>> logger)
    {
        _context = context;
        _dbSet = context.Set<T>();
        _logger = logger;
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting {EntityType} with id {Id}", typeof(T).Name, id);
        return await _dbSet.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, cancellationToken);
    }

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting all {EntityType} entities", typeof(T).Name);
        return await _dbSet.Where(e => !e.IsDeleted).ToListAsync(cancellationToken);
    }

    public async Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Creating new {EntityType} entity", typeof(T).Name);
        
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.Version = 1;
        
        var result = await _dbSet.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Created {EntityType} entity with id {Id}", typeof(T).Name, entity.Id);
        return result.Entity;
    }

    public async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Updating {EntityType} entity with id {Id}", typeof(T).Name, entity.Id);
        
        entity.UpdateTimestamp();
        
        _dbSet.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Updated {EntityType} entity with id {Id}", typeof(T).Name, entity.Id);
        return entity;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Soft deleting {EntityType} entity with id {Id}", typeof(T).Name, id);
        
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            entity.MarkAsDeleted();
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Soft deleted {EntityType} entity with id {Id}", typeof(T).Name, id);
        }
    }

    public async Task HardDeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Hard deleting {EntityType} entity with id {Id}", typeof(T).Name, id);
        
        var entity = await _dbSet.FindAsync(new object[] { id }, cancellationToken);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Hard deleted {EntityType} entity with id {Id}", typeof(T).Name, id);
        }
    }

    public async Task<IEnumerable<T>> QueryAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Querying {EntityType} entities with predicate", typeof(T).Name);
        return await _dbSet.Where(e => !e.IsDeleted).Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, 
        int pageSize, 
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting paged {EntityType} entities - Page {PageNumber}, Size {PageSize}", 
            typeof(T).Name, pageNumber, pageSize);

        var query = _dbSet.Where(e => !e.IsDeleted);

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        if (orderBy != null)
        {
            query = orderBy(query);
        }

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(e => !e.IsDeleted, cancellationToken);
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(e => !e.IsDeleted).CountAsync(predicate, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(e => e.Id == id && !e.IsDeleted, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(e => !e.IsDeleted).AnyAsync(predicate, cancellationToken);
    }

    public async Task<IEnumerable<T>> GetByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting {EntityType} entities with tag {Tag}", typeof(T).Name, tag);
        return await _dbSet.Where(e => !e.IsDeleted && e.Tags.Contains(tag)).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<T>> GetByTagsAsync(string[] tags, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting {EntityType} entities with tags {Tags}", typeof(T).Name, string.Join(", ", tags));
        return await _dbSet.Where(e => !e.IsDeleted && e.Tags.Any(t => tags.Contains(t))).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<T>> GetByCreatedByAsync(string createdBy, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting {EntityType} entities created by {CreatedBy}", typeof(T).Name, createdBy);
        return await _dbSet.Where(e => !e.IsDeleted && e.CreatedBy == createdBy).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<T>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting {EntityType} entities created between {StartDate} and {EndDate}", 
            typeof(T).Name, startDate, endDate);
        return await _dbSet.Where(e => !e.IsDeleted && e.CreatedAt >= startDate && e.CreatedAt <= endDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<T>> GetByUpdatedDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting {EntityType} entities updated between {StartDate} and {EndDate}", 
            typeof(T).Name, startDate, endDate);
        return await _dbSet.Where(e => !e.IsDeleted && e.UpdatedAt >= startDate && e.UpdatedAt <= endDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<T>> GetByMetadataAsync(string key, object value, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting {EntityType} entities with metadata {Key}={Value}", typeof(T).Name, key, value);
        
        // Note: This is a simplified implementation. For production, you might want to use JSON functions
        // or store metadata in a separate table for better querying performance
        var allEntities = await GetAllAsync(cancellationToken);
        return allEntities.Where(e => e.Metadata.TryGetValue(key, out var metadataValue) && 
                                    Equals(metadataValue, value));
    }

    public async Task RestoreAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Restoring {EntityType} entity with id {Id}", typeof(T).Name, id);
        
        var entity = await _dbSet.FirstOrDefaultAsync(e => e.Id == id && e.IsDeleted, cancellationToken);
        if (entity != null)
        {
            entity.Restore();
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Restored {EntityType} entity with id {Id}", typeof(T).Name, id);
        }
    }

    public async Task<IEnumerable<T>> GetDeletedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting all deleted {EntityType} entities", typeof(T).Name);
        return await _dbSet.Where(e => e.IsDeleted).ToListAsync(cancellationToken);
    }

    public IQueryable<T> GetQueryable()
    {
        return _dbSet.AsQueryable();
    }
} 