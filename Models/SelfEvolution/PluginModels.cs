using System.ComponentModel.DataAnnotations;
using Microsoft.CodeAnalysis;

namespace UltraGenericSystem.Models.SelfEvolution;

/// <summary>
/// Result of plugin loading operation
/// </summary>
public class PluginLoadResult
{
    public bool Success { get; set; }
    
    public LoadedPlugin? Plugin { get; set; }
    
    public List<string> Functions { get; set; } = new();
    
    public DateTime LoadTimestamp { get; set; }
    
    public string? ErrorMessage { get; set; }
    
    public string? AssemblyPath { get; set; }
    
    public List<string> Diagnostics { get; set; } = new();
}

/// <summary>
/// Result of plugin unloading operation
/// </summary>
public class PluginUnloadResult
{
    public bool Success { get; set; }
    
    public string PluginName { get; set; } = string.Empty;
    
    public DateTime UnloadTimestamp { get; set; }
    
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Information about a loaded plugin
/// </summary>
public class LoadedPlugin
{
    public string Name { get; set; } = string.Empty;
    
    public System.Reflection.Assembly Assembly { get; set; } = null!;
    
    public object Plugin { get; set; } = null!;
    
    public PluginInfo Info { get; set; } = null!;
    
    public DateTime LoadTimestamp { get; set; }
    
    public bool IsActive { get; set; }
    
    public PluginLoadOptions Options { get; set; } = new();
}

/// <summary>
/// Information about a plugin
/// </summary>
public class PluginInfo
{
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public string Version { get; set; } = "1.0.0";
    
    public string AssemblyPath { get; set; } = string.Empty;
    
    public DateTime LoadedAt { get; set; } = DateTime.UtcNow;
    
    public string AssemblyName { get; set; } = string.Empty;
    
    public List<PluginFunction> Functions { get; set; } = new();
}

/// <summary>
/// Information about a plugin function
/// </summary>
public class PluginFunction
{
    public string Name { get; set; } = string.Empty;
    
    public string ReturnType { get; set; } = string.Empty;
    
    public List<PluginParameter> Parameters { get; set; } = new();
}

/// <summary>
/// Information about a plugin parameter
/// </summary>
public class PluginParameter
{
    public string Name { get; set; } = string.Empty;
    
    public string Type { get; set; } = string.Empty;
    
    public bool IsOptional { get; set; }
}

/// <summary>
/// Options for plugin loading
/// </summary>
public class PluginLoadOptions
{
    public string Description { get; set; } = string.Empty;
    
    public string Version { get; set; } = "1.0.0";
    
    public bool AllowUnsafe { get; set; } = false;
    
    public Microsoft.CodeAnalysis.OptimizationLevel OptimizationLevel { get; set; } = Microsoft.CodeAnalysis.OptimizationLevel.Release;
    
    public bool ValidateSecurity { get; set; } = true;
    
    public bool EnableLogging { get; set; } = true;
}

/// <summary>
/// Result of plugin validation
/// </summary>
public class PluginValidationResult
{
    public bool IsValid { get; set; }
    
    public List<string> Errors { get; set; } = new();
    
    public List<string> Warnings { get; set; } = new();
    
    public Dictionary<string, object> ValidationData { get; set; } = new();
}

/// <summary>
/// Result of plugin template generation
/// </summary>
public class PluginTemplateResult
{
    public bool Success { get; set; }
    
    public string PluginCode { get; set; } = string.Empty;
    
    public string PluginName { get; set; } = string.Empty;
    
    public DateTime GenerationTimestamp { get; set; }
    
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Options for plugin template generation
/// </summary>
public class PluginTemplateOptions
{
    public string Namespace { get; set; } = "UltraGenericSystem.Plugins";
    
    public string BaseClass { get; set; } = "object";
    
    public bool IncludeLogging { get; set; } = true;
    
    public bool IncludeErrorHandling { get; set; } = true;
    
    public bool IncludeValidation { get; set; } = true;
    
    public string TemplateStyle { get; set; } = "Standard"; // Standard, Minimal, Comprehensive
}

/// <summary>
/// Result of plugin registration with kernel
/// </summary>
public class PluginRegistrationResult
{
    public bool Success { get; set; }
    
    public List<string> Functions { get; set; } = new();
    
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Result of OpenAPI generation
/// </summary>
public class OpenApiGenerationResult
{
    public bool Success { get; set; }
    
    public NSwag.OpenApiDocument? OpenApiDocument { get; set; }
    
    public string ApiName { get; set; } = string.Empty;
    
    public DateTime GenerationTimestamp { get; set; }
    
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Options for OpenAPI generation
/// </summary>
public class OpenApiGenerationOptions
{
    public string? BaseUrl { get; set; }
    
    public string Version { get; set; } = "1.0.0";
    
    public string Description { get; set; } = "AI-generated API";
    
    public bool EnableAuthentication { get; set; } = false;
    
    public string AuthenticationType { get; set; } = "Bearer"; // Bearer, API Key, OAuth2
    
    public bool GenerateExamples { get; set; } = true;
    
    public bool IncludeErrorResponses { get; set; } = true;
}

/// <summary>
/// Result of OpenAPI validation
/// </summary>
public class OpenApiValidationResult
{
    public bool IsValid { get; set; }
    
    public List<string> Errors { get; set; } = new();
    
    public List<string> Warnings { get; set; } = new();
    
    public Dictionary<string, object> ValidationData { get; set; } = new();
}

/// <summary>
/// Result of controller generation
/// </summary>
public class ControllerGenerationResult
{
    public bool Success { get; set; }
    
    public string ControllerCode { get; set; } = string.Empty;
    
    public Type ControllerType { get; set; } = null!;
    
    public string ControllerName { get; set; } = string.Empty;
    
    public DateTime GenerationTimestamp { get; set; }
    
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Options for controller generation
/// </summary>
public class ControllerGenerationOptions
{
    public bool UseAsyncAwait { get; set; } = true;
    
    public bool GenerateValidationAttributes { get; set; } = true;
    
    public bool AddLogging { get; set; } = true;
    
    public bool AddErrorHandling { get; set; } = true;
    
    public bool AddValidationAttributes { get; set; } = true;
    
    public string ControllerBaseClass { get; set; } = "ControllerBase";
    
    public bool GenerateDocumentation { get; set; } = true;
}

/// <summary>
/// Result of controller compilation
/// </summary>
public class ControllerCompilationResult
{
    public bool Success { get; set; }
    
    public Type ControllerType { get; set; } = null!;
    
    public string AssemblyPath { get; set; } = string.Empty;
    
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Result of endpoint registration
/// </summary>
public class EndpointRegistrationResult
{
    public bool Success { get; set; }
    
    public EndpointRegistration Registration { get; set; } = new();
    
    public List<EndpointInfo> Endpoints { get; set; } = new();
    
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Information about endpoint registration
/// </summary>
public class EndpointRegistration
{
    public Type ControllerType { get; set; } = null!;
    
    public string RoutePrefix { get; set; } = string.Empty;
    
    public DateTime RegistrationTimestamp { get; set; }
    
    public bool IsActive { get; set; }
}

/// <summary>
/// Information about an endpoint
/// </summary>
public class EndpointInfo
{
    public string Method { get; set; } = string.Empty; // GET, POST, PUT, DELETE, etc.
    
    public string Route { get; set; } = string.Empty;
    
    public string MethodName { get; set; } = string.Empty;
    
    public List<string> Parameters { get; set; } = new();
}

/// <summary>
/// Result of complete API generation
/// </summary>
public class ApiGenerationResult
{
    public bool Success { get; set; }
    
    public string ApiName { get; set; } = string.Empty;
    
    public NSwag.OpenApiDocument? OpenApiDocument { get; set; }
    
    public List<ControllerGenerationResult> Controllers { get; set; } = new();
    
    public List<EndpointRegistrationResult> Registrations { get; set; } = new();
    
    public DateTime GenerationTimestamp { get; set; }
    
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Options for complete API generation
/// </summary>
public class CompleteApiGenerationOptions
{
    public OpenApiGenerationOptions OpenApiOptions { get; set; } = new();
    
    public ControllerGenerationOptions ControllerOptions { get; set; } = new();
    
    public string? RoutePrefix { get; set; }
    
    public bool GenerateMultipleControllers { get; set; } = true;
    
    public bool EnableSwagger { get; set; } = true;
}

/// <summary>
/// Information about a generated API
/// </summary>
public class GeneratedApiInfo
{
    public string ApiName { get; set; } = string.Empty;
    
    public NSwag.OpenApiDocument? OpenApiDocument { get; set; }
    
    public DateTime GenerationTimestamp { get; set; }
}

/// <summary>
/// Information about a generated controller
/// </summary>
public class GeneratedControllerInfo
{
    public string ControllerName { get; set; } = string.Empty;
    
    public Type ControllerType { get; set; } = null!;
    
    public DateTime GenerationTimestamp { get; set; }
} 