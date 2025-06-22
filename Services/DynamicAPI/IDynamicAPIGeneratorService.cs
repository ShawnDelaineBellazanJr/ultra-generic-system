using Microsoft.AspNetCore.Mvc;
using UltraGenericSystem.Models;

namespace UltraGenericSystem.Services.DynamicAPI;

/// <summary>
/// Interface for dynamic API generation using OpenAPI and NSwag
/// </summary>
public interface IDynamicAPIGeneratorService
{
    /// <summary>
    /// Generates OpenAPI specification from LLM description
    /// </summary>
    Task<OpenAPIGenerationResult> GenerateOpenAPISpecAsync(
        string description,
        OpenAPIGenerationOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates C# controller from OpenAPI specification
    /// </summary>
    Task<ControllerGenerationResult> GenerateControllerAsync(
        string openApiSpec,
        ControllerGenerationOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a dynamically generated controller
    /// </summary>
    Task<bool> RegisterControllerAsync(
        string controllerCode,
        string controllerName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Unregisters a dynamic controller
    /// </summary>
    Task<bool> UnregisterControllerAsync(string controllerName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all registered dynamic controllers
    /// </summary>
    Task<List<DynamicControllerInfo>> GetRegisteredControllersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates OpenAPI specification
    /// </summary>
    Task<OpenAPIValidationResult> ValidateOpenAPISpecAsync(
        string openApiSpec,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// OpenAPI generation options
/// </summary>
public class OpenAPIGenerationOptions
{
    public string Version { get; set; } = "3.0.1";
    public string Title { get; set; } = "Dynamic API";
    public string Description { get; set; } = "Dynamically generated API";
    public string BasePath { get; set; } = "/api";
    public List<string> Tags { get; set; } = new();
    public Dictionary<string, object> CustomProperties { get; set; } = new();
    public bool EnableSwaggerUI { get; set; } = true;
    public bool EnableAuthentication { get; set; } = false;
    public string AuthenticationType { get; set; } = "Bearer";
}

/// <summary>
/// Controller generation options
/// </summary>
public class ControllerGenerationOptions
{
    public string Namespace { get; set; } = "UltraGenericSystem.Controllers.Dynamic";
    public string BaseController { get; set; } = "ControllerBase";
    public bool EnableLogging { get; set; } = true;
    public bool EnableValidation { get; set; } = true;
    public bool EnableCaching { get; set; } = false;
    public List<string> UsingStatements { get; set; } = new();
    public Dictionary<string, string> CustomAttributes { get; set; } = new();
    public string ImplementationPattern { get; set; } = "SkillBased"; // SkillBased, AgentBased, Custom
}

/// <summary>
/// OpenAPI generation result
/// </summary>
public class OpenAPIGenerationResult
{
    public bool Success { get; set; }
    public string? OpenAPISpec { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
    public TimeSpan GenerationTime { get; set; }
}

/// <summary>
/// Controller generation result
/// </summary>
public class ControllerGenerationResult
{
    public bool Success { get; set; }
    public string? ControllerCode { get; set; }
    public string? ControllerName { get; set; }
    public List<string> Endpoints { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
    public TimeSpan GenerationTime { get; set; }
}

/// <summary>
/// OpenAPI validation result
/// </summary>
public class OpenAPIValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public Dictionary<string, object> ValidationDetails { get; set; } = new();
}

/// <summary>
/// Dynamic controller information
/// </summary>
public class DynamicControllerInfo
{
    public string Name { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public List<string> Endpoints { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModified { get; set; }
    public bool IsActive { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// API endpoint information
/// </summary>
public class APIEndpointInfo
{
    public string Path { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public List<ParameterInfo> Parameters { get; set; } = new();
    public ResponseInfo? Response { get; set; }
}

/// <summary>
/// Parameter information
/// </summary>
public class ParameterInfo
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty; // query, path, header, cookie
    public bool Required { get; set; }
    public string? DefaultValue { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// Response information
/// </summary>
public class ResponseInfo
{
    public int StatusCode { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public string? Schema { get; set; }
} 