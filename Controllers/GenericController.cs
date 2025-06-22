using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UltraGenericSystem.Models;
using UltraGenericSystem.Services;

namespace UltraGenericSystem.Controllers;

/// <summary>
/// Generic controller that handles all entity types automatically
/// Provides full CRUD operations for any entity that inherits from BaseEntity
/// </summary>
[ApiController]
[Route("api/v1/{entityType}")]
public class GenericController<T> : ControllerBase where T : BaseEntity, new()
{
    private readonly IAgentService<T> _agentService;
    private readonly ILogger<GenericController<T>> _logger;

    public GenericController(IAgentService<T> agentService, ILogger<GenericController<T>> logger)
    {
        _agentService = agentService;
        _logger = logger;
    }

    /// <summary>
    /// Get all entities of the specified type
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<T>>> GetAllAsync(
        [FromQuery] int? page = null,
        [FromQuery] int? pageSize = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool? ascending = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting all {EntityType} entities", typeof(T).Name);

            var request = new AgentQueryRequest
            {
                EntityType = typeof(T),
                Operation = "GetAll",
                Parameters = new Dictionary<string, object>
                {
                    ["PageNumber"] = page ?? 1,
                    ["PageSize"] = pageSize ?? 100,
                    ["SortBy"] = sortBy ?? "CreatedAt",
                    ["Ascending"] = ascending ?? false
                }
            };

            var response = await _agentService.QueryAsync(request, cancellationToken);
            
            if (!response.IsSuccess)
            {
                _logger.LogWarning("Failed to get {EntityType} entities: {Error}", typeof(T).Name, response.Error);
                return BadRequest(new { error = response.Error, duration = response.Duration, logs = response.ExecutionLog });
            }

            _logger.LogInformation("Successfully retrieved {Count} {EntityType} entities in {Duration}ms", 
                response.Data?.Count() ?? 0, typeof(T).Name, response.Duration.TotalMilliseconds);

            return Ok(new
            {
                data = response.Data,
                metadata = new
                {
                    entityType = typeof(T).Name,
                    count = response.Data?.Count() ?? 0,
                    duration = response.Duration.TotalMilliseconds,
                    executionLog = response.ExecutionLog
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all {EntityType} entities", typeof(T).Name);
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Get a specific entity by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<T>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting {EntityType} with ID {Id}", typeof(T).Name, id);

            var request = new AgentRequest<T>
            {
                Operation = "GetById",
                Data = new { Id = id },
            };

            var response = await _agentService.ProcessAsync(request, cancellationToken);
            
            if (!response.IsSuccess)
            {
                _logger.LogWarning("Failed to get {EntityType} with ID {Id}: {Error}", typeof(T).Name, id, response.Error);
                return NotFound(new { error = response.Error, duration = response.Duration, logs = response.ExecutionLog });
            }

            if (response.Data == null)
            {
                return NotFound(new { error = "Entity not found", id });
            }

            _logger.LogInformation("Successfully retrieved {EntityType} with ID {Id} in {Duration}ms", 
                typeof(T).Name, id, response.Duration.TotalMilliseconds);

            return Ok(new
            {
                data = response.Data,
                metadata = new
                {
                    entityType = typeof(T).Name,
                    id = response.Data.Id,
                    duration = response.Duration.TotalMilliseconds,
                    executionLog = response.ExecutionLog
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting {EntityType} with ID {Id}", typeof(T).Name, id);
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Create a new entity
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<T>> CreateAsync([FromBody] T entity, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating new {EntityType}", typeof(T).Name);

            entity.OnCreated(GetCurrentUser());

            var request = new AgentCreateRequest<T>
            {
                Entity = entity,
                Operation = "Create"
            };

            var response = await _agentService.CreateAsync(request, cancellationToken);
            
            if (!response.IsSuccess)
            {
                _logger.LogWarning("Failed to create {EntityType}: {Error}", typeof(T).Name, response.Error);
                return BadRequest(new { error = response.Error, duration = response.Duration, logs = response.ExecutionLog });
            }

            _logger.LogInformation("Successfully created {EntityType} with ID {Id} in {Duration}ms", 
                typeof(T).Name, response.Data?.Id, response.Duration.TotalMilliseconds);

            return CreatedAtAction(
                nameof(GetByIdAsync), 
                new { entityType = typeof(T).Name.ToLower(), id = response.Data!.Id }, 
                new
                {
                    data = response.Data,
                    metadata = new
                    {
                        entityType = typeof(T).Name,
                        id = response.Data.Id,
                        duration = response.Duration.TotalMilliseconds,
                        executionLog = response.ExecutionLog
                    }
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating {EntityType}", typeof(T).Name);
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing entity
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<T>> UpdateAsync(Guid id, [FromBody] T entity, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating {EntityType} with ID {Id}", typeof(T).Name, id);

            entity.Id = id;
            entity.OnUpdated();

            var request = new AgentUpdateRequest<T>
            {
                Entity = entity,
                Operation = "Update"
            };

            var response = await _agentService.UpdateAsync(request, cancellationToken);
            
            if (!response.IsSuccess)
            {
                _logger.LogWarning("Failed to update {EntityType} with ID {Id}: {Error}", typeof(T).Name, id, response.Error);
                return BadRequest(new { error = response.Error, duration = response.Duration, logs = response.ExecutionLog });
            }

            _logger.LogInformation("Successfully updated {EntityType} with ID {Id} in {Duration}ms", 
                typeof(T).Name, id, response.Duration.TotalMilliseconds);

            return Ok(new
            {
                data = response.Data,
                metadata = new
                {
                    entityType = typeof(T).Name,
                    id = response.Data?.Id,
                    duration = response.Duration.TotalMilliseconds,
                    executionLog = response.ExecutionLog
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating {EntityType} with ID {Id}", typeof(T).Name, id);
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Delete an entity (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting {EntityType} with ID {Id}", typeof(T).Name, id);

            var request = new AgentDeleteRequest
            {
                Id = id,
                Operation = "Delete"
            };

            var response = await _agentService.DeleteAsync(request, cancellationToken);
            
            if (!response.IsSuccess)
            {
                _logger.LogWarning("Failed to delete {EntityType} with ID {Id}: {Error}", typeof(T).Name, id, response.Error);
                return BadRequest(new { error = response.Error, duration = response.Duration, logs = response.ExecutionLog });
            }

            _logger.LogInformation("Successfully deleted {EntityType} with ID {Id} in {Duration}ms", 
                typeof(T).Name, id, response.Duration.TotalMilliseconds);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting {EntityType} with ID {Id}", typeof(T).Name, id);
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Query entities with filters
    /// </summary>
    [HttpPost("query")]
    public async Task<ActionResult<IEnumerable<T>>> QueryAsync([FromBody] QueryRequest queryRequest, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Querying {EntityType} with filters", typeof(T).Name);

            var request = new AgentQueryRequest
            {
                EntityType = typeof(T),
                Operation = "Query",
                Parameters = queryRequest.Filters ?? new Dictionary<string, object>(),
                PageNumber = queryRequest.Page ?? 1,
                PageSize = queryRequest.PageSize ?? 100,
                SortBy = queryRequest.SortBy ?? "CreatedAt",
                SortDirection = (queryRequest.Ascending ?? false) ? "Asc" : "Desc"
            };

            var response = await _agentService.QueryAsync(request, cancellationToken);
            
            if (!response.IsSuccess)
            {
                _logger.LogWarning("Failed to query {EntityType}: {Error}", typeof(T).Name, response.Error);
                return BadRequest(new { error = response.Error, duration = response.Duration, logs = response.ExecutionLog });
            }

            _logger.LogInformation("Successfully queried {EntityType} - found {Count} results in {Duration}ms", 
                typeof(T).Name, response.Data?.Count() ?? 0, response.Duration.TotalMilliseconds);

            return Ok(new
            {
                data = response.Data,
                metadata = new
                {
                    entityType = typeof(T).Name,
                    count = response.Data?.Count() ?? 0,
                    duration = response.Duration.TotalMilliseconds,
                    executionLog = response.ExecutionLog
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying {EntityType}", typeof(T).Name);
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Bulk operations
    /// </summary>
    [HttpPost("bulk")]
    public async Task<ActionResult<BulkOperationResult>> BulkOperationAsync([FromBody] BulkOperationRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Performing bulk operation on {EntityType}: {Operation}", typeof(T).Name, request.Operation);

            var agentRequest = new AgentRequest<T>
            {
                Operation = request.Operation,
                Data = request.Entities,
                Metadata = new Dictionary<string, object>
                {
                    ["RequestId"] = Guid.NewGuid().ToString(),
                    ["UserAgent"] = Request.Headers.UserAgent.ToString(),
                    ["BulkOperation"] = true,
                    ["EntityCount"] = request.Entities?.Count() ?? 0
                }
            };

            var response = await _agentService.ProcessAsync(agentRequest, cancellationToken);
            
            if (!response.IsSuccess)
            {
                _logger.LogWarning("Failed to perform bulk operation on {EntityType}: {Error}", typeof(T).Name, response.Error);
                return BadRequest(new { error = response.Error, duration = response.Duration, logs = response.ExecutionLog });
            }

            var result = new BulkOperationResult
            {
                Success = true,
                ProcessedCount = request.Entities?.Count() ?? 0,
                Duration = response.Duration,
                ExecutionLog = response.ExecutionLog
            };

            _logger.LogInformation("Successfully performed bulk operation on {EntityType} - processed {Count} entities in {Duration}ms", 
                typeof(T).Name, result.ProcessedCount, response.Duration.TotalMilliseconds);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing bulk operation on {EntityType}", typeof(T).Name);
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    private string GetCurrentUser()
    {
        // In a real application, this would get the current user from the authentication context
        return User?.Identity?.Name ?? "System";
    }

    private string GetClientIpAddress()
    {
        return Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }
}

/// <summary>
/// Query request model
/// </summary>
public class QueryRequest
{
    public Dictionary<string, object>? Filters { get; set; }
    public int? Page { get; set; }
    public int? PageSize { get; set; }
    public string? SortBy { get; set; }
    public bool? Ascending { get; set; }
}

/// <summary>
/// Bulk operation request model
/// </summary>
public class BulkOperationRequest
{
    public string Operation { get; set; } = string.Empty;
    public IEnumerable<object>? Entities { get; set; }
}

/// <summary>
/// Bulk operation result model
/// </summary>
public class BulkOperationResult
{
    public bool Success { get; set; }
    public int ProcessedCount { get; set; }
    public TimeSpan Duration { get; set; }
    public List<string> ExecutionLog { get; set; } = new();
} 