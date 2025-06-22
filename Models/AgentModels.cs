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

/// <summary>
/// Azure AI Agent configuration
/// </summary>
public class AzureAIAgentConfig
{
    /// <summary>
    /// Azure OpenAI endpoint
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Azure OpenAI API key
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Model name (alias for ModelDeploymentName)
    /// </summary>
    public string ModelName { get; set; } = "gpt-4";

    /// <summary>
    /// Model deployment name
    /// </summary>
    public string ModelDeploymentName { get; set; } = "gpt-4";

    /// <summary>
    /// Model deployment name for embeddings
    /// </summary>
    public string EmbeddingDeploymentName { get; set; } = "text-embedding-ada-002";

    /// <summary>
    /// Maximum tokens for completion
    /// </summary>
    public int MaxTokens { get; set; } = 4000;

    /// <summary>
    /// Temperature for generation
    /// </summary>
    public double Temperature { get; set; } = 0.7;

    /// <summary>
    /// Top P for generation
    /// </summary>
    public double TopP { get; set; } = 0.9;

    /// <summary>
    /// Frequency penalty
    /// </summary>
    public double FrequencyPenalty { get; set; } = 0.0;

    /// <summary>
    /// Presence penalty
    /// </summary>
    public double PresencePenalty { get; set; } = 0.0;

    /// <summary>
    /// Whether to enable streaming
    /// </summary>
    public bool EnableStreaming { get; set; } = true;

    /// <summary>
    /// Whether to enable function calling
    /// </summary>
    public bool EnableFunctionCalling { get; set; } = true;

    /// <summary>
    /// Whether to enable vision capabilities
    /// </summary>
    public bool EnableVision { get; set; } = false;

    /// <summary>
    /// Maximum retry attempts (alias for RetryConfig.MaxRetries)
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Retry delay in seconds (alias for RetryConfig.BaseDelay)
    /// </summary>
    public int RetryDelay { get; set; } = 1;

    /// <summary>
    /// Custom headers for API calls
    /// </summary>
    public Dictionary<string, string> CustomHeaders { get; set; } = new();

    /// <summary>
    /// Retry configuration
    /// </summary>
    public AzureAIRetryConfig RetryConfig { get; set; } = new();

    /// <summary>
    /// Memory configuration
    /// </summary>
    public AzureAIMemoryConfig MemoryConfig { get; set; } = new();
}

/// <summary>
/// Azure AI retry configuration
/// </summary>
public class AzureAIRetryConfig
{
    /// <summary>
    /// Maximum retry attempts
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Base delay between retries
    /// </summary>
    public TimeSpan BaseDelay { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Maximum delay between retries
    /// </summary>
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Whether to use exponential backoff
    /// </summary>
    public bool UseExponentialBackoff { get; set; } = true;

    /// <summary>
    /// HTTP status codes to retry on
    /// </summary>
    public List<int> RetryableStatusCodes { get; set; } = new() { 429, 500, 502, 503, 504 };
}

/// <summary>
/// Azure AI memory configuration
/// </summary>
public class AzureAIMemoryConfig
{
    /// <summary>
    /// Whether to enable memory
    /// </summary>
    public bool EnableMemory { get; set; } = true;

    /// <summary>
    /// Memory type to use
    /// </summary>
    public string MemoryType { get; set; } = "Volatile"; // Volatile, Persistent, Vector

    /// <summary>
    /// Maximum memory entries
    /// </summary>
    public int MaxMemoryEntries { get; set; } = 1000;

    /// <summary>
    /// Maximum conversation history entries
    /// </summary>
    public int MaxConversationHistory { get; set; } = 100;

    /// <summary>
    /// Memory retention period
    /// </summary>
    public TimeSpan MemoryRetentionPeriod { get; set; } = TimeSpan.FromHours(24);

    /// <summary>
    /// Whether to enable memory search
    /// </summary>
    public bool EnableMemorySearch { get; set; } = true;

    /// <summary>
    /// Whether to enable embeddings
    /// </summary>
    public bool EnableEmbeddings { get; set; } = true;

    /// <summary>
    /// Memory search similarity threshold
    /// </summary>
    public double MemorySearchThreshold { get; set; } = 0.8;
}

/// <summary>
/// Azure AI Agent request
/// </summary>
public class AzureAIAgentRequest
{
    /// <summary>
    /// User message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// System prompt
    /// </summary>
    public string? SystemPrompt { get; set; }

    /// <summary>
    /// Conversation history
    /// </summary>
    public List<AzureAIConversationMessage> ConversationHistory { get; set; } = new();

    /// <summary>
    /// Function definitions
    /// </summary>
    public List<AzureAIFunctionDefinition> Functions { get; set; } = new();

    /// <summary>
    /// Tool calls to execute
    /// </summary>
    public List<AzureAIToolCall> ToolCalls { get; set; } = new();

    /// <summary>
    /// Additional parameters
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new();

    /// <summary>
    /// Whether to enable streaming
    /// </summary>
    public bool EnableStreaming { get; set; } = true;

    /// <summary>
    /// User ID for tracking
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Session ID for conversation tracking
    /// </summary>
    public string SessionId { get; set; } = Guid.NewGuid().ToString();
}

/// <summary>
/// Azure AI conversation message
/// </summary>
public class AzureAIConversationMessage
{
    /// <summary>
    /// Role of the message sender
    /// </summary>
    public string Role { get; set; } = string.Empty; // system, user, assistant, tool

    /// <summary>
    /// Message content
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Message timestamp
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Message ID
    /// </summary>
    public string MessageId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Tool calls in this message
    /// </summary>
    public List<AzureAIToolCall> ToolCalls { get; set; } = new();

    /// <summary>
    /// Tool call results
    /// </summary>
    public List<AzureAIToolCallResult> ToolCallResults { get; set; } = new();
}

/// <summary>
/// Azure AI function definition
/// </summary>
public class AzureAIFunctionDefinition
{
    /// <summary>
    /// Function name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Function description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Function parameters schema
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new();

    /// <summary>
    /// Whether the function is required
    /// </summary>
    public bool Required { get; set; } = false;
}

/// <summary>
/// Azure AI tool call
/// </summary>
public class AzureAIToolCall
{
    /// <summary>
    /// Tool call ID
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Tool type
    /// </summary>
    public string Type { get; set; } = "function";

    /// <summary>
    /// Function call details
    /// </summary>
    public AzureAIFunctionCall Function { get; set; } = new();
}

/// <summary>
/// Azure AI function call
/// </summary>
public class AzureAIFunctionCall
{
    /// <summary>
    /// Function name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Function arguments
    /// </summary>
    public string Arguments { get; set; } = string.Empty;
}

/// <summary>
/// Azure AI tool call result
/// </summary>
public class AzureAIToolCallResult
{
    /// <summary>
    /// Tool call ID
    /// </summary>
    public string ToolCallId { get; set; } = string.Empty;

    /// <summary>
    /// Tool type
    /// </summary>
    public string Type { get; set; } = "function";

    /// <summary>
    /// Function call result
    /// </summary>
    public AzureAIFunctionCallResult Function { get; set; } = new();
}

/// <summary>
/// Azure AI function call result
/// </summary>
public class AzureAIFunctionCallResult
{
    /// <summary>
    /// Function name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Function result content
    /// </summary>
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// Azure AI Agent response
/// </summary>
public class AzureAIAgentResponse
{
    /// <summary>
    /// Response message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Tool calls made
    /// </summary>
    public List<AzureAIToolCall> ToolCalls { get; set; } = new();

    /// <summary>
    /// Tool call results
    /// </summary>
    public List<AzureAIToolCallResult> ToolCallResults { get; set; } = new();

    /// <summary>
    /// Usage statistics
    /// </summary>
    public AzureAIUsage Usage { get; set; } = new();

    /// <summary>
    /// Whether the response is complete
    /// </summary>
    public bool IsComplete { get; set; } = true;

    /// <summary>
    /// Response ID
    /// </summary>
    public string ResponseId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Response timestamp
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Azure AI usage statistics
/// </summary>
public class AzureAIUsage
{
    /// <summary>
    /// Number of prompt tokens
    /// </summary>
    public int PromptTokens { get; set; }

    /// <summary>
    /// Number of completion tokens
    /// </summary>
    public int CompletionTokens { get; set; }

    /// <summary>
    /// Total number of tokens
    /// </summary>
    public int TotalTokens { get; set; }

    /// <summary>
    /// Model used
    /// </summary>
    public string Model { get; set; } = string.Empty;
}

/// <summary>
/// Azure AI streaming response
/// </summary>
public class AzureAIStreamingResponse
{
    /// <summary>
    /// Streaming chunk content
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is the final chunk
    /// </summary>
    public bool IsFinal { get; set; } = false;

    /// <summary>
    /// Chunk ID
    /// </summary>
    public string ChunkId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Chunk timestamp
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Tool calls in this chunk
    /// </summary>
    public List<AzureAIToolCall> ToolCalls { get; set; } = new();
}

/// <summary>
/// Azure AI Agent capabilities
/// </summary>
public class AzureAIAgentCapabilities
{
    /// <summary>
    /// Whether the agent supports text generation
    /// </summary>
    public bool SupportsTextGeneration { get; set; } = true;

    /// <summary>
    /// Whether the agent supports function calling
    /// </summary>
    public bool SupportsFunctionCalling { get; set; } = true;

    /// <summary>
    /// Whether the agent supports vision
    /// </summary>
    public bool SupportsVision { get; set; } = false;

    /// <summary>
    /// Whether the agent supports streaming
    /// </summary>
    public bool SupportsStreaming { get; set; } = true;

    /// <summary>
    /// Whether the agent supports memory
    /// </summary>
    public bool SupportsMemory { get; set; } = true;

    /// <summary>
    /// Whether the agent supports embeddings
    /// </summary>
    public bool SupportsEmbeddings { get; set; } = true;

    /// <summary>
    /// Maximum input tokens
    /// </summary>
    public int MaxInputTokens { get; set; } = 128000;

    /// <summary>
    /// Maximum output tokens
    /// </summary>
    public int MaxOutputTokens { get; set; } = 4096;

    /// <summary>
    /// Supported models
    /// </summary>
    public List<string> SupportedModels { get; set; } = new();
}

/// <summary>
/// Request model for conversation demo
/// </summary>
public class ConversationDemoRequest
{
    public string? Task { get; set; } = "Create a simple web application";
    public bool EnableRealAgents { get; set; } = false;
    public int ConversationLength { get; set; } = 10;
}

/// <summary>
/// Request model for Chain of Thought agent
/// </summary>
public class ChainOfThoughtRequest
{
    /// <summary>
    /// User's natural language request
    /// </summary>
    public string UserRequest { get; set; } = string.Empty;

    /// <summary>
    /// Whether to enable detailed reasoning output
    /// </summary>
    public bool EnableDetailedReasoning { get; set; } = true;

    /// <summary>
    /// Maximum number of API calls to make
    /// </summary>
    public int MaxApiCalls { get; set; } = 10;

    /// <summary>
    /// Timeout for the entire operation
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);
}

/// <summary>
/// Response model for Chain of Thought agent
/// </summary>
public class ChainOfThoughtResponse
{
    /// <summary>
    /// Whether the operation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Original user request
    /// </summary>
    public string UserRequest { get; set; } = string.Empty;

    /// <summary>
    /// Result of the Chain of Thought processing
    /// </summary>
    public string Result { get; set; } = string.Empty;

    /// <summary>
    /// Error message if operation failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Timestamp of the operation
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Number of API calls made
    /// </summary>
    public int ApiCallsMade { get; set; } = 0;

    /// <summary>
    /// Detailed reasoning steps
    /// </summary>
    public List<string> ReasoningSteps { get; set; } = new();

    /// <summary>
    /// API calls executed
    /// </summary>
    public List<string> ApiCalls { get; set; } = new();
}

/// <summary>
/// Response model for API capabilities
/// </summary>
public class ApiCapabilitiesResponse
{
    /// <summary>
    /// Whether the operation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Available API capabilities summary
    /// </summary>
    public string Capabilities { get; set; } = string.Empty;

    /// <summary>
    /// Error message if operation failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Timestamp of the operation
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Number of available endpoints
    /// </summary>
    public int EndpointCount { get; set; } = 0;

    /// <summary>
    /// List of available endpoint categories
    /// </summary>
    public List<string> Categories { get; set; } = new();
}

/// <summary>
/// Request model for continuous loop conversation
/// </summary>
public class ContinuousLoopRequest
{
    public string? Task { get; set; } = "Create a continuous self-improvement loop";
    public int? MaxIterations { get; set; } = 5;
    public double? ConvergenceThreshold { get; set; } = 0.8;
    public bool EnableMetaLearning { get; set; } = true;
}

/// <summary>
/// Result of a single iteration in the continuous loop
/// </summary>
public class IterationResult
{
    public int IterationNumber { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration { get; set; }
    public string? Plan { get; set; }
    public string? Execution { get; set; }
    public string? Evaluation { get; set; }
    public string? Reflection { get; set; }
    public double ConvergenceScore { get; set; }
    public bool ShouldContinue { get; set; }
    public string? Summary { get; set; }
    public List<string> ConversationHistory { get; set; } = new();
    public Dictionary<string, object>? MetaData { get; set; }
} 