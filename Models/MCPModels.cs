using System.Text.Json.Serialization;

namespace UltraGenericSystem.Models;

/// <summary>
/// MCP Server configuration
/// </summary>
public class MCPServer
{
    /// <summary>
    /// Server name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Server endpoint URL
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Server capabilities
    /// </summary>
    public List<string> Capabilities { get; set; } = new();

    /// <summary>
    /// Authentication configuration
    /// </summary>
    public MCPAuth? Authentication { get; set; }

    /// <summary>
    /// Connection status
    /// </summary>
    public bool IsConnected { get; set; }

    /// <summary>
    /// Last connection timestamp
    /// </summary>
    public DateTime? LastConnected { get; set; }
}

/// <summary>
/// MCP Authentication configuration
/// </summary>
public class MCPAuth
{
    /// <summary>
    /// Authentication type
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Authentication token
    /// </summary>
    public string? Token { get; set; }

    /// <summary>
    /// Additional authentication parameters
    /// </summary>
    public Dictionary<string, object>? Parameters { get; set; }
}

/// <summary>
/// MCP Request model
/// </summary>
public class MCPRequest
{
    /// <summary>
    /// Request method
    /// </summary>
    [JsonPropertyName("method")]
    public string Method { get; set; } = string.Empty;

    /// <summary>
    /// Request parameters
    /// </summary>
    [JsonPropertyName("params")]
    public Dictionary<string, object> Params { get; set; } = new();

    /// <summary>
    /// Request ID
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// JSON-RPC version
    /// </summary>
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; set; } = "2.0";
}

/// <summary>
/// MCP Response model
/// </summary>
public class MCPResponse
{
    /// <summary>
    /// Whether the request was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Response result
    /// </summary>
    public Dictionary<string, object>? Result { get; set; }

    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Response timestamp
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Request ID
    /// </summary>
    public string? RequestId { get; set; }
}

/// <summary>
/// MCP Capability model
/// </summary>
public class MCPCapability
{
    /// <summary>
    /// Capability name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Capability type
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Capability schema
    /// </summary>
    public object? Schema { get; set; }

    /// <summary>
    /// Capability description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Capability parameters
    /// </summary>
    public Dictionary<string, object>? Parameters { get; set; }

    /// <summary>
    /// Capability timestamp
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// MCP Discovery Result
/// </summary>
public class MCPDiscoveryResult
{
    /// <summary>
    /// Whether discovery was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Discovered server
    /// </summary>
    public MCPServer? Server { get; set; }

    /// <summary>
    /// Discovered capabilities
    /// </summary>
    public List<MCPCapability> Capabilities { get; set; } = new();

    /// <summary>
    /// Server schema
    /// </summary>
    public Dictionary<string, object>? Schema { get; set; }

    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Discovery timestamp
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Discovery duration
    /// </summary>
    public TimeSpan? Duration { get; set; }
}

/// <summary>
/// Plugin generation request for self-evolving system
/// </summary>
public class PluginGenerationRequest
{
    /// <summary>
    /// MCP server information
    /// </summary>
    public MCPServer Server { get; set; } = new();

    /// <summary>
    /// MCP discovery result
    /// </summary>
    public MCPDiscoveryResult Discovery { get; set; } = new();

    /// <summary>
    /// Goal for plugin generation
    /// </summary>
    public string Goal { get; set; } = string.Empty;

    /// <summary>
    /// Target capabilities to generate plugins for
    /// </summary>
    public List<string> TargetCapabilities { get; set; } = new();

    /// <summary>
    /// Generation preferences
    /// </summary>
    public Dictionary<string, object> Preferences { get; set; } = new();
}

/// <summary>
/// Plugin generation result from self-evolving system
/// </summary>
public class PluginGenerationResult
{
    /// <summary>
    /// Whether generation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if generation failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Generated plugins
    /// </summary>
    public List<GeneratedPlugin> GeneratedPlugins { get; set; } = new();

    /// <summary>
    /// Evolution insights
    /// </summary>
    public List<string> EvolutionInsights { get; set; } = new();

    /// <summary>
    /// Generation timestamp
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Generated plugin information
/// </summary>
public class GeneratedPlugin
{
    /// <summary>
    /// Plugin name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Plugin version
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Plugin methods
    /// </summary>
    public List<string> Methods { get; set; } = new();

    /// <summary>
    /// Compilation status
    /// </summary>
    public string CompilationStatus { get; set; } = "Pending";

    /// <summary>
    /// Integration status
    /// </summary>
    public string IntegrationStatus { get; set; } = "Pending";

    /// <summary>
    /// Generated code
    /// </summary>
    public string GeneratedCode { get; set; } = string.Empty;
}

/// <summary>
/// Plugin Method model
/// </summary>
public class PluginMethod
{
    /// <summary>
    /// Method name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Method description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Method parameters
    /// </summary>
    public List<PluginParameter> Parameters { get; set; } = new();

    /// <summary>
    /// Return type
    /// </summary>
    public string ReturnType { get; set; } = "string";

    /// <summary>
    /// SK Function attribute
    /// </summary>
    public bool IsSKFunction { get; set; } = true;

    /// <summary>
    /// Method implementation
    /// </summary>
    public string Implementation { get; set; } = string.Empty;
}

/// <summary>
/// Plugin Parameter model
/// </summary>
public class PluginParameter
{
    /// <summary>
    /// Parameter name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Parameter type
    /// </summary>
    public string Type { get; set; } = "string";

    /// <summary>
    /// Parameter description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Whether parameter is required
    /// </summary>
    public bool Required { get; set; } = true;

    /// <summary>
    /// Default value
    /// </summary>
    public object? DefaultValue { get; set; }
}

/// <summary>
/// GitHub Operation Result
/// </summary>
public class GitHubOperationResult
{
    /// <summary>
    /// Whether operation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Operation name
    /// </summary>
    public string Operation { get; set; } = string.Empty;

    /// <summary>
    /// Operation result
    /// </summary>
    public object? Result { get; set; }

    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Operation timestamp
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Operation duration
    /// </summary>
    public TimeSpan? Duration { get; set; }

    /// <summary>
    /// Request ID for tracking
    /// </summary>
    public string RequestId { get; set; } = Guid.NewGuid().ToString();
}

/// <summary>
/// GitHub Repository Information
/// </summary>
public class GitHubRepositoryInfo
{
    /// <summary>
    /// Repository owner
    /// </summary>
    public string Owner { get; set; } = string.Empty;

    /// <summary>
    /// Repository name
    /// </summary>
    public string Repository { get; set; } = string.Empty;

    /// <summary>
    /// Repository data
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// Whether operation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Operation timestamp
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// GitHub Issues Result
/// </summary>
public class GitHubIssuesResult
{
    /// <summary>
    /// Repository owner
    /// </summary>
    public string Owner { get; set; } = string.Empty;

    /// <summary>
    /// Repository name
    /// </summary>
    public string Repository { get; set; } = string.Empty;

    /// <summary>
    /// Issues data
    /// </summary>
    public object? Issues { get; set; }

    /// <summary>
    /// Whether operation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Operation timestamp
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Number of issues
    /// </summary>
    public int IssueCount { get; set; }
}

/// <summary>
/// GitHub Issue Result
/// </summary>
public class GitHubIssueResult
{
    /// <summary>
    /// Repository owner
    /// </summary>
    public string Owner { get; set; } = string.Empty;

    /// <summary>
    /// Repository name
    /// </summary>
    public string Repository { get; set; } = string.Empty;

    /// <summary>
    /// Issue data
    /// </summary>
    public object? Issue { get; set; }

    /// <summary>
    /// Whether operation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Operation timestamp
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Issue number
    /// </summary>
    public int? IssueNumber { get; set; }
}

/// <summary>
/// MCP Self-Evolution Context
/// </summary>
public class MCPSelfEvolutionContext
{
    /// <summary>
    /// Current MCP servers
    /// </summary>
    public List<MCPServer> ConnectedServers { get; set; } = new();

    /// <summary>
    /// Discovered capabilities
    /// </summary>
    public List<MCPCapability> DiscoveredCapabilities { get; set; } = new();

    /// <summary>
    /// Generated plugins
    /// </summary>
    public List<GeneratedPlugin> GeneratedPlugins { get; set; } = new();

    /// <summary>
    /// Learning insights
    /// </summary>
    public List<string> LearningInsights { get; set; } = new();

    /// <summary>
    /// Evolution metrics
    /// </summary>
    public Dictionary<string, object> EvolutionMetrics { get; set; } = new();

    /// <summary>
    /// Context timestamp
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// MCP Plugin Generation Strategy
/// </summary>
public class MCPPluginGenerationStrategy
{
    /// <summary>
    /// Strategy name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Strategy description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Code generation approach
    /// </summary>
    public string CodeGenerationApproach { get; set; } = "template_based";

    /// <summary>
    /// Template patterns
    /// </summary>
    public List<string> TemplatePatterns { get; set; } = new();

    /// <summary>
    /// Error handling strategy
    /// </summary>
    public string ErrorHandlingStrategy { get; set; } = "graceful_degradation";

    /// <summary>
    /// Testing approach
    /// </summary>
    public string TestingApproach { get; set; } = "generative_testing";

    /// <summary>
    /// Integration strategy
    /// </summary>
    public string IntegrationStrategy { get; set; } = "automatic_registration";

    /// <summary>
    /// Strategy parameters
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new();
} 