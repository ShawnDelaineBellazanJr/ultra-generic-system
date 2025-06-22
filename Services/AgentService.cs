using Microsoft.Extensions.Logging;
using UltraGenericSystem.Models;
using UltraGenericSystem.Repositories;
using UltraGenericSystem.Services;

namespace UltraGenericSystem.Services;

/// <summary>
/// Generic agent service implementation for all entities in the ultra-generic system
/// </summary>
/// <typeparam name="T">The entity type that inherits from BaseEntity</typeparam>
public class AgentService<T> : IAgentService<T> where T : BaseEntity
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAgentOrchestrator _orchestrator;
    private readonly ILogger<AgentService<T>> _logger;

    public AgentService(IUnitOfWork unitOfWork, IAgentOrchestrator orchestrator, ILogger<AgentService<T>> logger)
    {
        _unitOfWork = unitOfWork;
        _orchestrator = orchestrator;
        _logger = logger;
    }

    public async Task<AgentResponse<T>> ProcessAsync(AgentRequest<T> request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Processing agent request for {EntityType} - Operation: {Operation}", typeof(T).Name, request.Operation);

            var context = new AgentExecutionContext
            {
                EntityType = typeof(T),
                Operation = request.Operation,
                Data = request.Data,
                Metadata = request.Metadata,
                UserId = request.UserId,
                CorrelationId = request.CorrelationId,
                Priority = request.Priority
            };

            var result = await _orchestrator.ExecuteAsync<T>(context, cancellationToken);
            
            _logger.LogInformation("Processed agent request for {EntityType} - Operation: {Operation} - Success: {Success}", 
                typeof(T).Name, request.Operation, result.IsSuccess);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing agent request for {EntityType}", typeof(T).Name);
            return AgentResponse<T>.Failure($"Processing failed: {ex.Message}");
        }
    }

    public async Task<AgentResponse<IEnumerable<T>>> QueryAsync(AgentQueryRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Querying {EntityType} entities - Operation: {Operation}", typeof(T).Name, request.Operation);

            var context = new AgentExecutionContext
            {
                EntityType = typeof(T),
                Operation = request.Operation,
                Data = request.Parameters,
                Metadata = new Dictionary<string, object>
                {
                    ["PageNumber"] = request.PageNumber,
                    ["PageSize"] = request.PageSize,
                    ["SortBy"] = request.SortBy,
                    ["SortDirection"] = request.SortDirection,
                    ["Filters"] = request.Filters,
                    ["IncludeDeleted"] = request.IncludeDeleted
                }
            };

            var result = await _orchestrator.ExecuteQueryAsync<T>(context, cancellationToken);
            
            _logger.LogInformation("Queried {EntityType} entities - Operation: {Operation} - Count: {Count}", 
                typeof(T).Name, request.Operation, result.Data?.Count() ?? 0);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying {EntityType}", typeof(T).Name);
            return AgentResponse<IEnumerable<T>>.Failure($"Query failed: {ex.Message}");
        }
    }

    public async Task<AgentResponse<T>> CreateAsync(AgentCreateRequest<T> request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Creating {EntityType} entity", typeof(T).Name);

            var context = new AgentExecutionContext
            {
                EntityType = typeof(T),
                Operation = request.Operation,
                Data = request.Entity,
                UserId = request.UserId,
                Metadata = new Dictionary<string, object>
                {
                    ["ValidationOptions"] = request.ValidationOptions
                }
            };

            var result = await _orchestrator.ExecuteAsync<T>(context, cancellationToken);
            
            _logger.LogInformation("Created {EntityType} entity - Success: {Success}", typeof(T).Name, result.IsSuccess);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating {EntityType}", typeof(T).Name);
            return AgentResponse<T>.Failure($"Creation failed: {ex.Message}");
        }
    }

    public async Task<AgentResponse<T>> UpdateAsync(AgentUpdateRequest<T> request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Updating {EntityType} entity with id {Id}", typeof(T).Name, request.Entity.Id);

            var context = new AgentExecutionContext
            {
                EntityType = typeof(T),
                Operation = request.Operation,
                Data = request.Entity,
                UserId = request.UserId,
                Metadata = new Dictionary<string, object>
                {
                    ["ValidationOptions"] = request.ValidationOptions,
                    ["CheckConcurrency"] = request.CheckConcurrency
                }
            };

            var result = await _orchestrator.ExecuteAsync<T>(context, cancellationToken);
            
            _logger.LogInformation("Updated {EntityType} entity with id {Id} - Success: {Success}", 
                typeof(T).Name, request.Entity.Id, result.IsSuccess);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating {EntityType} with id {Id}", typeof(T).Name, request.Entity.Id);
            return AgentResponse<T>.Failure($"Update failed: {ex.Message}");
        }
    }

    public async Task<AgentResponse<bool>> DeleteAsync(AgentDeleteRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Deleting {EntityType} entity with id {Id}", typeof(T).Name, request.Id);

            var context = new AgentExecutionContext
            {
                EntityType = typeof(T),
                Operation = request.Operation,
                Data = new { Id = request.Id },
                UserId = request.UserId,
                Metadata = new Dictionary<string, object>
                {
                    ["HardDelete"] = request.HardDelete
                }
            };

            var result = await _orchestrator.ExecuteDeleteAsync(context, cancellationToken);
            
            _logger.LogInformation("Deleted {EntityType} entity with id {Id} - Success: {Success}", 
                typeof(T).Name, request.Id, result.IsSuccess);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting {EntityType} with id {Id}", typeof(T).Name, request.Id);
            return AgentResponse<bool>.Failure($"Deletion failed: {ex.Message}");
        }
    }

    public async Task<AgentResponse<T>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting {EntityType} entity with id {Id}", typeof(T).Name, id);

            var request = new AgentRequest<T>
            {
                Operation = "GetById",
                Data = new { Id = id }
            };

            return await ProcessAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting {EntityType} with id {Id}", typeof(T).Name, id);
            return AgentResponse<T>.Failure($"GetById failed: {ex.Message}");
        }
    }

    public async Task<AgentResponse<IEnumerable<T>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting all {EntityType} entities", typeof(T).Name);

            var request = new AgentRequest<T>
            {
                Operation = "GetAll"
            };

            var result = await ProcessAsync(request, cancellationToken);
            return AgentResponse<IEnumerable<T>>.Success(
                result.Data != null ? new[] { result.Data } : new T[0],
                result.Duration,
                result.ExecutionLog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all {EntityType}", typeof(T).Name);
            return AgentResponse<IEnumerable<T>>.Failure($"GetAll failed: {ex.Message}");
        }
    }

    public async Task<AgentResponse<PaginatedResult<T>>> GetPagedAsync(
        int pageNumber, 
        int pageSize, 
        string? sortBy = null, 
        string? sortDirection = null,
        Dictionary<string, object>? filters = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting paged {EntityType} entities - Page {PageNumber}, Size {PageSize}", 
                typeof(T).Name, pageNumber, pageSize);

            var queryRequest = new AgentQueryRequest
            {
                EntityType = typeof(T),
                Operation = "GetPaged",
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy ?? "CreatedAt",
                SortDirection = sortDirection ?? "Desc",
                Filters = filters ?? new Dictionary<string, object>()
            };

            var result = await QueryAsync(queryRequest, cancellationToken);
            
            if (!result.IsSuccess)
            {
                return AgentResponse<PaginatedResult<T>>.Failure(result.Error, result.Duration, result.ExecutionLog);
            }

            // For now, we'll use a simple implementation. In a real system, you'd want to get the total count
            // from the repository and create a proper paginated result
            var paginatedResult = new PaginatedResult<T>
            {
                Items = result.Data ?? new List<T>(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = result.Data?.Count() ?? 0
            };

            return AgentResponse<PaginatedResult<T>>.Success(paginatedResult, result.Duration, result.ExecutionLog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged {EntityType}", typeof(T).Name);
            return AgentResponse<PaginatedResult<T>>.Failure($"GetPaged failed: {ex.Message}");
        }
    }

    public async Task<AgentResponse<IEnumerable<T>>> GetByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting {EntityType} entities with tag {Tag}", typeof(T).Name, tag);

            var request = new AgentRequest<T>
            {
                Operation = "GetByTag",
                Data = new { Tag = tag }
            };

            var result = await ProcessAsync(request, cancellationToken);
            return AgentResponse<IEnumerable<T>>.Success(
                result.Data != null ? new[] { result.Data } : new T[0],
                result.Duration,
                result.ExecutionLog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting {EntityType} by tag {Tag}", typeof(T).Name, tag);
            return AgentResponse<IEnumerable<T>>.Failure($"GetByTag failed: {ex.Message}");
        }
    }

    public async Task<AgentResponse<IEnumerable<T>>> GetByTagsAsync(string[] tags, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting {EntityType} entities with tags {Tags}", typeof(T).Name, string.Join(", ", tags));

            var request = new AgentRequest<T>
            {
                Operation = "GetByTags",
                Data = new { Tags = tags }
            };

            var result = await ProcessAsync(request, cancellationToken);
            return AgentResponse<IEnumerable<T>>.Success(
                result.Data != null ? new[] { result.Data } : new T[0],
                result.Duration,
                result.ExecutionLog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting {EntityType} by tags", typeof(T).Name);
            return AgentResponse<IEnumerable<T>>.Failure($"GetByTags failed: {ex.Message}");
        }
    }

    public async Task<AgentResponse<IEnumerable<T>>> GetByCreatedByAsync(string createdBy, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting {EntityType} entities created by {CreatedBy}", typeof(T).Name, createdBy);

            var request = new AgentRequest<T>
            {
                Operation = "GetByCreatedBy",
                Data = new { CreatedBy = createdBy }
            };

            var result = await ProcessAsync(request, cancellationToken);
            return AgentResponse<IEnumerable<T>>.Success(
                result.Data != null ? new[] { result.Data } : new T[0],
                result.Duration,
                result.ExecutionLog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting {EntityType} by created by {CreatedBy}", typeof(T).Name, createdBy);
            return AgentResponse<IEnumerable<T>>.Failure($"GetByCreatedBy failed: {ex.Message}");
        }
    }

    public async Task<AgentResponse<IEnumerable<T>>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting {EntityType} entities between {StartDate} and {EndDate}", typeof(T).Name, startDate, endDate);

            var request = new AgentRequest<T>
            {
                Operation = "GetByDateRange",
                Data = new { StartDate = startDate, EndDate = endDate }
            };

            var result = await ProcessAsync(request, cancellationToken);
            return AgentResponse<IEnumerable<T>>.Success(
                result.Data != null ? new[] { result.Data } : new T[0],
                result.Duration,
                result.ExecutionLog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting {EntityType} by date range", typeof(T).Name);
            return AgentResponse<IEnumerable<T>>.Failure($"GetByDateRange failed: {ex.Message}");
        }
    }

    public async Task<AgentResponse<IEnumerable<T>>> GetByMetadataAsync(string key, object value, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting {EntityType} entities with metadata {Key}={Value}", typeof(T).Name, key, value);

            var request = new AgentRequest<T>
            {
                Operation = "GetByMetadata",
                Data = new { Key = key, Value = value }
            };

            var result = await ProcessAsync(request, cancellationToken);
            return AgentResponse<IEnumerable<T>>.Success(
                result.Data != null ? new[] { result.Data } : new T[0],
                result.Duration,
                result.ExecutionLog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting {EntityType} by metadata", typeof(T).Name);
            return AgentResponse<IEnumerable<T>>.Failure($"GetByMetadata failed: {ex.Message}");
        }
    }

    public async Task<AgentResponse<T>> RestoreAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Restoring {EntityType} entity with id {Id}", typeof(T).Name, id);

            var request = new AgentRequest<T>
            {
                Operation = "Restore",
                Data = new { Id = id }
            };

            return await ProcessAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring {EntityType} with id {Id}", typeof(T).Name, id);
            return AgentResponse<T>.Failure($"Restore failed: {ex.Message}");
        }
    }

    public async Task<AgentResponse<IEnumerable<T>>> GetDeletedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting deleted {EntityType} entities", typeof(T).Name);

            var request = new AgentRequest<T>
            {
                Operation = "GetDeleted"
            };

            var result = await ProcessAsync(request, cancellationToken);
            return AgentResponse<IEnumerable<T>>.Success(
                result.Data != null ? new[] { result.Data } : new T[0],
                result.Duration,
                result.ExecutionLog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting deleted {EntityType}", typeof(T).Name);
            return AgentResponse<IEnumerable<T>>.Failure($"GetDeleted failed: {ex.Message}");
        }
    }

    public async Task<AgentResponse<int>> CountAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Counting {EntityType} entities", typeof(T).Name);

            var request = new AgentRequest<T>
            {
                Operation = "Count"
            };

            var result = await ProcessAsync(request, cancellationToken);
            
            if (result.IsSuccess && result.Data != null)
            {
                // This is a simplified implementation. In a real system, you'd want to get the actual count
                return AgentResponse<int>.Success(1, result.Duration, result.ExecutionLog);
            }

            return AgentResponse<int>.Success(0, result.Duration, result.ExecutionLog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error counting {EntityType}", typeof(T).Name);
            return AgentResponse<int>.Failure($"Count failed: {ex.Message}");
        }
    }

    public async Task<AgentResponse<bool>> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Checking if {EntityType} entity with id {Id} exists", typeof(T).Name, id);

            var request = new AgentRequest<T>
            {
                Operation = "Exists",
                Data = new { Id = id }
            };

            var result = await ProcessAsync(request, cancellationToken);
            return AgentResponse<bool>.Success(result.IsSuccess && result.Data != null, result.Duration, result.ExecutionLog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if {EntityType} with id {Id} exists", typeof(T).Name, id);
            return AgentResponse<bool>.Failure($"Exists failed: {ex.Message}");
        }
    }

    public async Task<AgentResponse<BulkOperationResult<T>>> BulkCreateAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Bulk creating {Count} {EntityType} entities", entities.Count(), typeof(T).Name);

            var request = new AgentRequest<T>
            {
                Operation = "BulkCreate",
                Data = new { Entities = entities }
            };

            var result = await ProcessAsync(request, cancellationToken);
            
            // This is a simplified implementation. In a real system, you'd want to process each entity
            // and return detailed results
            var bulkResult = new BulkOperationResult<T>
            {
                SuccessCount = result.IsSuccess ? entities.Count() : 0,
                FailureCount = result.IsSuccess ? 0 : entities.Count(),
                SuccessResults = result.IsSuccess ? entities : new List<T>(),
                Errors = result.IsSuccess ? new List<BulkOperationError>() : new List<BulkOperationError>
                {
                    new BulkOperationError { Index = 0, Error = result.Error }
                }
            };

            return AgentResponse<BulkOperationResult<T>>.Success(bulkResult, result.Duration, result.ExecutionLog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk creating {EntityType}", typeof(T).Name);
            return AgentResponse<BulkOperationResult<T>>.Failure($"BulkCreate failed: {ex.Message}");
        }
    }

    public async Task<AgentResponse<BulkOperationResult<T>>> BulkUpdateAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Bulk updating {Count} {EntityType} entities", entities.Count(), typeof(T).Name);

            var request = new AgentRequest<T>
            {
                Operation = "BulkUpdate",
                Data = new { Entities = entities }
            };

            var result = await ProcessAsync(request, cancellationToken);
            
            // This is a simplified implementation. In a real system, you'd want to process each entity
            // and return detailed results
            var bulkResult = new BulkOperationResult<T>
            {
                SuccessCount = result.IsSuccess ? entities.Count() : 0,
                FailureCount = result.IsSuccess ? 0 : entities.Count(),
                SuccessResults = result.IsSuccess ? entities : new List<T>(),
                Errors = result.IsSuccess ? new List<BulkOperationError>() : new List<BulkOperationError>
                {
                    new BulkOperationError { Index = 0, Error = result.Error }
                }
            };

            return AgentResponse<BulkOperationResult<T>>.Success(bulkResult, result.Duration, result.ExecutionLog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk updating {EntityType}", typeof(T).Name);
            return AgentResponse<BulkOperationResult<T>>.Failure($"BulkUpdate failed: {ex.Message}");
        }
    }

    public async Task<AgentResponse<BulkOperationResult<Guid>>> BulkDeleteAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Bulk deleting {Count} {EntityType} entities", ids.Count(), typeof(T).Name);

            var request = new AgentRequest<T>
            {
                Operation = "BulkDelete",
                Data = new { Ids = ids }
            };

            var result = await ProcessAsync(request, cancellationToken);
            
            // This is a simplified implementation. In a real system, you'd want to process each ID
            // and return detailed results
            var bulkResult = new BulkOperationResult<Guid>
            {
                SuccessCount = result.IsSuccess ? ids.Count() : 0,
                FailureCount = result.IsSuccess ? 0 : ids.Count(),
                SuccessResults = result.IsSuccess ? ids : new List<Guid>(),
                Errors = result.IsSuccess ? new List<BulkOperationError>() : new List<BulkOperationError>
                {
                    new BulkOperationError { Index = 0, Error = result.Error }
                }
            };

            return AgentResponse<BulkOperationResult<Guid>>.Success(bulkResult, result.Duration, result.ExecutionLog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk deleting {EntityType}", typeof(T).Name);
            return AgentResponse<BulkOperationResult<Guid>>.Failure($"BulkDelete failed: {ex.Message}");
        }
    }
} 