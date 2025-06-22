using UltraGenericSystem.Models;

namespace UltraGenericSystem.Services;

/// <summary>
/// Generic agent service interface for all entities in the ultra-generic system
/// </summary>
/// <typeparam name="T">The entity type that inherits from BaseEntity</typeparam>
public interface IAgentService<T> where T : BaseEntity
{
    /// <summary>
    /// Processes a generic agent request
    /// </summary>
    Task<AgentResponse<T>> ProcessAsync(AgentRequest<T> request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries entities using agent orchestration
    /// </summary>
    Task<AgentResponse<IEnumerable<T>>> QueryAsync(AgentQueryRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new entity using agent orchestration
    /// </summary>
    Task<AgentResponse<T>> CreateAsync(AgentCreateRequest<T> request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity using agent orchestration
    /// </summary>
    Task<AgentResponse<T>> UpdateAsync(AgentUpdateRequest<T> request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity using agent orchestration
    /// </summary>
    Task<AgentResponse<bool>> DeleteAsync(AgentDeleteRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an entity by ID using agent orchestration
    /// </summary>
    Task<AgentResponse<T>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities using agent orchestration
    /// </summary>
    Task<AgentResponse<IEnumerable<T>>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets entities with pagination using agent orchestration
    /// </summary>
    Task<AgentResponse<PaginatedResult<T>>> GetPagedAsync(
        int pageNumber, 
        int pageSize, 
        string? sortBy = null, 
        string? sortDirection = null,
        Dictionary<string, object>? filters = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets entities by tag using agent orchestration
    /// </summary>
    Task<AgentResponse<IEnumerable<T>>> GetByTagAsync(string tag, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets entities by multiple tags using agent orchestration
    /// </summary>
    Task<AgentResponse<IEnumerable<T>>> GetByTagsAsync(string[] tags, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets entities created by a specific user using agent orchestration
    /// </summary>
    Task<AgentResponse<IEnumerable<T>>> GetByCreatedByAsync(string createdBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets entities within a date range using agent orchestration
    /// </summary>
    Task<AgentResponse<IEnumerable<T>>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets entities with metadata using agent orchestration
    /// </summary>
    Task<AgentResponse<IEnumerable<T>>> GetByMetadataAsync(string key, object value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Restores a soft-deleted entity using agent orchestration
    /// </summary>
    Task<AgentResponse<T>> RestoreAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all soft-deleted entities using agent orchestration
    /// </summary>
    Task<AgentResponse<IEnumerable<T>>> GetDeletedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts entities using agent orchestration
    /// </summary>
    Task<AgentResponse<int>> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an entity exists using agent orchestration
    /// </summary>
    Task<AgentResponse<bool>> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs bulk operations using agent orchestration
    /// </summary>
    Task<AgentResponse<BulkOperationResult<T>>> BulkCreateAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs bulk update operations using agent orchestration
    /// </summary>
    Task<AgentResponse<BulkOperationResult<T>>> BulkUpdateAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs bulk delete operations using agent orchestration
    /// </summary>
    Task<AgentResponse<BulkOperationResult<Guid>>> BulkDeleteAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of bulk operations
/// </summary>
/// <typeparam name="T">Result type</typeparam>
public class BulkOperationResult<T>
{
    /// <summary>
    /// Number of successful operations
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Number of failed operations
    /// </summary>
    public int FailureCount { get; set; }

    /// <summary>
    /// Successful results
    /// </summary>
    public IEnumerable<T> SuccessResults { get; set; } = new List<T>();

    /// <summary>
    /// Failed operations with errors
    /// </summary>
    public IEnumerable<BulkOperationError> Errors { get; set; } = new List<BulkOperationError>();

    /// <summary>
    /// Total number of operations
    /// </summary>
    public int TotalCount => SuccessCount + FailureCount;
}

/// <summary>
/// Error information for bulk operations
/// </summary>
public class BulkOperationError
{
    /// <summary>
    /// Index of the failed operation
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Error message
    /// </summary>
    public string Error { get; set; } = string.Empty;

    /// <summary>
    /// Data that caused the error
    /// </summary>
    public object? Data { get; set; }
} 