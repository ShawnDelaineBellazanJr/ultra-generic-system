using System.Text.Json;

namespace UltraGenericSystem.Models;

/// <summary>
/// Execution context for agent operations
/// </summary>
public class AgentExecutionContext
{
    /// <summary>
    /// Type of entity being operated on
    /// </summary>
    public Type EntityType { get; set; } = typeof(BaseEntity);

    /// <summary>
    /// Operation being performed
    /// </summary>
    public string Operation { get; set; } = string.Empty;

    /// <summary>
    /// Data for the operation
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// Additional metadata for the operation
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// When the execution started
    /// </summary>
    public DateTime StartTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Duration of the execution
    /// </summary>
    public TimeSpan Duration { get; set; } = TimeSpan.Zero;

    /// <summary>
    /// Execution log for debugging and monitoring
    /// </summary>
    public List<string> ExecutionLog { get; set; } = new();

    /// <summary>
    /// Result of the operation
    /// </summary>
    public object? Result { get; set; }

    /// <summary>
    /// Whether the operation was successful
    /// </summary>
    public bool IsSuccess { get; set; } = true;

    /// <summary>
    /// Error message if operation failed
    /// </summary>
    public string Error { get; set; } = string.Empty;

    /// <summary>
    /// User performing the operation
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Priority of the operation
    /// </summary>
    public int Priority { get; set; } = 0;

    /// <summary>
    /// Retry count for failed operations
    /// </summary>
    public int RetryCount { get; set; } = 0;

    /// <summary>
    /// Maximum retry attempts
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Adds a log entry
    /// </summary>
    public void AddLog(string message)
    {
        ExecutionLog.Add($"[{DateTime.UtcNow:HH:mm:ss.fff}] {message}");
    }

    /// <summary>
    /// Sets the result and marks as successful
    /// </summary>
    public void SetResult(object result)
    {
        Result = result;
        IsSuccess = true;
        Duration = DateTime.UtcNow - StartTime;
    }

    /// <summary>
    /// Sets an error and marks as failed
    /// </summary>
    public void SetError(string error)
    {
        Error = error;
        IsSuccess = false;
        Duration = DateTime.UtcNow - StartTime;
    }
}

/// <summary>
/// Generic agent request
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public class AgentRequest<T> where T : BaseEntity
{
    /// <summary>
    /// Operation to perform
    /// </summary>
    public string Operation { get; set; } = string.Empty;

    /// <summary>
    /// Data for the operation
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// User performing the operation
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Priority of the operation
    /// </summary>
    public int Priority { get; set; } = 0;
}

/// <summary>
/// Generic agent response
/// </summary>
/// <typeparam name="T">Response data type</typeparam>
public class AgentResponse<T>
{
    /// <summary>
    /// Whether the operation was successful
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Response data
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Error message if operation failed
    /// </summary>
    public string Error { get; set; } = string.Empty;

    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Duration of the operation
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Execution log
    /// </summary>
    public List<string> ExecutionLog { get; set; } = new();

    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string CorrelationId { get; set; } = string.Empty;

    /// <summary>
    /// When the response was created
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a successful response
    /// </summary>
    public static AgentResponse<T> Success(T data, TimeSpan duration = default, List<string>? executionLog = null)
    {
        return new AgentResponse<T>
        {
            IsSuccess = true,
            Data = data,
            Duration = duration,
            ExecutionLog = executionLog ?? new List<string>(),
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a failed response
    /// </summary>
    public static AgentResponse<T> Failure(string error, TimeSpan duration = default, List<string>? executionLog = null)
    {
        return new AgentResponse<T>
        {
            IsSuccess = false,
            Error = error,
            Duration = duration,
            ExecutionLog = executionLog ?? new List<string>(),
            Timestamp = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Query request for agent operations
/// </summary>
public class AgentQueryRequest
{
    /// <summary>
    /// Type of entity to query
    /// </summary>
    public Type EntityType { get; set; } = typeof(BaseEntity);

    /// <summary>
    /// Operation to perform
    /// </summary>
    public string Operation { get; set; } = string.Empty;

    /// <summary>
    /// Query parameters
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new();

    /// <summary>
    /// Page number for pagination
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Page size for pagination
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Sort field
    /// </summary>
    public string SortBy { get; set; } = "CreatedAt";

    /// <summary>
    /// Sort direction
    /// </summary>
    public string SortDirection { get; set; } = "Desc";

    /// <summary>
    /// Filter criteria
    /// </summary>
    public Dictionary<string, object> Filters { get; set; } = new();

    /// <summary>
    /// Include deleted entities
    /// </summary>
    public bool IncludeDeleted { get; set; } = false;

    /// <summary>
    /// Query type
    /// </summary>
    public string QueryType { get; set; } = string.Empty;

    /// <summary>
    /// Page
    /// </summary>
    public int Page { get => PageNumber; set => PageNumber = value; }
}

/// <summary>
/// Create request for agent operations
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public class AgentCreateRequest<T> where T : BaseEntity
{
    /// <summary>
    /// Entity to create
    /// </summary>
    public T Entity { get; set; } = default!;

    /// <summary>
    /// Operation type
    /// </summary>
    public string Operation { get; set; } = "Create";

    /// <summary>
    /// User creating the entity
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Validation options
    /// </summary>
    public ValidationOptions ValidationOptions { get; set; } = new();

    /// <summary>
    /// Created by
    /// </summary>
    public string CreatedBy { get => UserId; set => UserId = value; }

    /// <summary>
    /// Validation level
    /// </summary>
    public string ValidationLevel { get; set; } = string.Empty;
}

/// <summary>
/// Update request for agent operations
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public class AgentUpdateRequest<T> where T : BaseEntity
{
    /// <summary>
    /// Entity to update
    /// </summary>
    public T Entity { get; set; } = default!;

    /// <summary>
    /// Operation type
    /// </summary>
    public string Operation { get; set; } = "Update";

    /// <summary>
    /// User updating the entity
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Validation options
    /// </summary>
    public ValidationOptions ValidationOptions { get; set; } = new();

    /// <summary>
    /// Whether to check for optimistic concurrency
    /// </summary>
    public bool CheckConcurrency { get; set; } = true;

    /// <summary>
    /// Updated by
    /// </summary>
    public string UpdatedBy { get => UserId; set => UserId = value; }

    /// <summary>
    /// Validation level
    /// </summary>
    public string ValidationLevel { get; set; } = string.Empty;

    /// <summary>
    /// Optimistic concurrency
    /// </summary>
    public bool OptimisticConcurrency { get => CheckConcurrency; set => CheckConcurrency = value; }
}

/// <summary>
/// Delete request for agent operations
/// </summary>
public class AgentDeleteRequest
{
    /// <summary>
    /// ID of entity to delete
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Operation type
    /// </summary>
    public string Operation { get; set; } = "Delete";

    /// <summary>
    /// User deleting the entity
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Whether to perform hard delete
    /// </summary>
    public bool HardDelete { get; set; } = false;

    /// <summary>
    /// Deleted by
    /// </summary>
    public string DeletedBy { get => UserId; set => UserId = value; }

    /// <summary>
    /// Whether to perform soft delete
    /// </summary>
    public bool SoftDelete { get; set; } = false;

    /// <summary>
    /// Whether to perform cascade delete
    /// </summary>
    public bool CascadeDelete { get; set; } = false;
}

/// <summary>
/// Validation options for agent operations
/// </summary>
public class ValidationOptions
{
    /// <summary>
    /// Whether to validate required fields
    /// </summary>
    public bool ValidateRequired { get; set; } = true;

    /// <summary>
    /// Whether to validate field lengths
    /// </summary>
    public bool ValidateLengths { get; set; } = true;

    /// <summary>
    /// Whether to validate business rules
    /// </summary>
    public bool ValidateBusinessRules { get; set; } = true;

    /// <summary>
    /// Whether to validate relationships
    /// </summary>
    public bool ValidateRelationships { get; set; } = true;

    /// <summary>
    /// Custom validation rules
    /// </summary>
    public Dictionary<string, object> CustomRules { get; set; } = new();
}

/// <summary>
/// Pagination result
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public class PaginatedResult<T>
{
    /// <summary>
    /// Items in the current page
    /// </summary>
    public IEnumerable<T> Items { get; set; } = new List<T>();

    /// <summary>
    /// Total count of items
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Page size
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    /// <summary>
    /// Whether there is a previous page
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Whether there is a next page
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;
}

/// <summary>
/// Advanced orchestration configuration with structured data support
/// </summary>
public class AdvancedOrchestrationConfig
{
    public bool EnableStructuredData { get; set; } = false;
    public bool EnableResponseCallbacks { get; set; } = true;
    public bool EnableHumanInTheLoop { get; set; } = false;
    public bool EnableCustomTransforms { get; set; } = false;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);
    public bool EnableCancellation { get; set; } = true;
    public Dictionary<string, object> CustomSettings { get; set; } = new();
}

/// <summary>
/// Structured input model for orchestration
/// </summary>
public class StructuredInput<T>
{
    public T Data { get; set; } = default!;
    public Dictionary<string, object> Metadata { get; set; } = new();
    public string? Context { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Structured output model for orchestration
/// </summary>
public class StructuredOutput<T>
{
    public T Data { get; set; } = default!;
    public Dictionary<string, object> Metadata { get; set; } = new();
    public List<string> Citations { get; set; } = new();
    public string? Summary { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool Success { get; set; } = true;
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Response callback configuration
/// </summary>
public class ResponseCallbackConfig
{
    public bool EnableLogging { get; set; } = true;
    public bool EnableUIUpdates { get; set; } = false;
    public bool EnableMetrics { get; set; } = true;
    public string? CustomFormat { get; set; }
    public Dictionary<string, object> CustomSettings { get; set; } = new();
}

/// <summary>
/// Human-in-the-loop configuration
/// </summary>
public class HumanInTheLoopConfig
{
    public bool EnableUserInput { get; set; } = false;
    public bool EnableApprovalWorkflow { get; set; } = false;
    public TimeSpan UserInputTimeout { get; set; } = TimeSpan.FromMinutes(2);
    public string? DefaultUserResponse { get; set; }
    public List<string> AllowedUserActions { get; set; } = new();
    public Dictionary<string, object> CustomSettings { get; set; } = new();
}

/// <summary>
/// Custom transform configuration
/// </summary>
public class CustomTransformConfig
{
    public string? InputTransformScript { get; set; }
    public string? OutputTransformScript { get; set; }
    public Dictionary<string, object> TransformParameters { get; set; } = new();
    public bool EnableValidation { get; set; } = true;
    public bool EnableCaching { get; set; } = false;
}

/// <summary>
/// Advanced agent request with orchestration features
/// </summary>
public class AdvancedAgentRequest<TInput, TOutput>
{
    public TInput Input { get; set; } = default!;
    public string Operation { get; set; } = string.Empty;
    public AdvancedOrchestrationConfig OrchestrationConfig { get; set; } = new();
    public ResponseCallbackConfig ResponseCallbackConfig { get; set; } = new();
    public HumanInTheLoopConfig HumanInTheLoopConfig { get; set; } = new();
    public CustomTransformConfig CustomTransformConfig { get; set; } = new();
    public Dictionary<string, object> Parameters { get; set; } = new();
    public CancellationToken CancellationToken { get; set; } = CancellationToken.None;
}

/// <summary>
/// Advanced agent response with orchestration features
/// </summary>
public class AdvancedAgentResponse<TOutput>
{
    public TOutput Output { get; set; } = default!;
    public bool Success { get; set; } = true;
    public string? ErrorMessage { get; set; }
    public TimeSpan ExecutionTime { get; set; }
    public List<string> AgentResponses { get; set; } = new();
    public List<string> UserInteractions { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Orchestration execution context with advanced features
/// </summary>
public class AdvancedOrchestrationContext<TInput, TOutput>
{
    public StructuredInput<TInput> StructuredInput { get; set; } = null!;
    public AdvancedOrchestrationConfig Config { get; set; } = new();
    public ResponseCallbackConfig ResponseConfig { get; set; } = new();
    public HumanInTheLoopConfig HumanConfig { get; set; } = new();
    public CustomTransformConfig TransformConfig { get; set; } = new();
    public Dictionary<string, object> State { get; set; } = new();
    public List<string> ExecutionLog { get; set; } = new();
    public CancellationToken CancellationToken { get; set; } = CancellationToken.None;
    
    public void LogExecution(string message)
    {
        ExecutionLog.Add($"[{DateTime.UtcNow:HH:mm:ss}] {message}");
    }
    
    public void SetState(string key, object value)
    {
        State[key] = value;
    }
    
    public T? GetState<T>(string key)
    {
        return State.TryGetValue(key, out var value) ? (T)value : default;
    }
}

/// <summary>
/// Generic entity analysis result
/// </summary>
public class EntityAnalysisResult
{
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public Dictionary<string, object> Analysis { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public double Confidence { get; set; } = 1.0;
    public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Generic workflow execution result
/// </summary>
public class WorkflowExecutionResult
{
    public string WorkflowId { get; set; } = string.Empty;
    public string WorkflowName { get; set; } = string.Empty;
    public bool Success { get; set; } = true;
    public List<string> Steps { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public Dictionary<string, object> Outputs { get; set; } = new();
    public TimeSpan TotalExecutionTime { get; set; }
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime EndTime { get; set; } = DateTime.UtcNow;
} 