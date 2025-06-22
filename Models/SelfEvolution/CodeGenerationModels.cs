using System.ComponentModel.DataAnnotations;

namespace UltraGenericSystem.Models.SelfEvolution;

/// <summary>
/// Represents a code generation template
/// </summary>
public class CodeTemplate : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string TemplateType { get; set; } = string.Empty; // Entity, Controller, Service, Repository, etc.
    
    [Required]
    public string TemplateContent { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public new string Description { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
    
    public new int Version { get; set; } = 1;
    
    [MaxLength(100)]
    public new string? Tags { get; set; }
}

/// <summary>
/// Request for code generation
/// </summary>
public class CodeGenerationRequest
{
    [Required]
    public string TemplateName { get; set; } = string.Empty;
    
    [Required]
    public Dictionary<string, object> Parameters { get; set; } = new();
    
    public string? OutputPath { get; set; }
    
    public bool CompileImmediately { get; set; } = false;
    
    public bool RegisterInDI { get; set; } = false;
    
    [MaxLength(500)]
    public string? Description { get; set; }
}

/// <summary>
/// Result of code generation
/// </summary>
public class CodeGenerationResult
{
    public bool Success { get; set; }
    
    public string GeneratedCode { get; set; } = string.Empty;
    
    public string? OutputPath { get; set; }
    
    public List<string> Warnings { get; set; } = new();
    
    public List<string> Errors { get; set; } = new();
    
    public CompilationResult? CompilationResult { get; set; }
    
    public TimeSpan GenerationTime { get; set; }
    
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Result of code compilation
/// </summary>
public class CompilationResult
{
    public bool Success { get; set; }
    
    public byte[]? AssemblyBytes { get; set; }
    
    public List<DiagnosticInfo> Diagnostics { get; set; } = new();
    
    public TimeSpan CompilationTime { get; set; }
    
    public string? AssemblyName { get; set; }
    
    public List<string> ReferencedAssemblies { get; set; } = new();
}

/// <summary>
/// Compilation diagnostic information
/// </summary>
public class DiagnosticInfo
{
    public string Id { get; set; } = string.Empty;
    
    public string Message { get; set; } = string.Empty;
    
    public string Severity { get; set; } = string.Empty; // Error, Warning, Info
    
    public int Line { get; set; }
    
    public int Column { get; set; }
    
    public string? FilePath { get; set; }
}

/// <summary>
/// Dynamic plugin information
/// </summary>
public class DynamicPlugin : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string PluginType { get; set; } = string.Empty; // Skill, Function, Agent, etc.
    
    public string SourceCode { get; set; } = string.Empty;
    
    public byte[]? CompiledAssembly { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public bool IsLoaded { get; set; } = false;
    
    [MaxLength(500)]
    public new string Description { get; set; } = string.Empty;
    
    public new string? Version { get; set; }
    
    public Dictionary<string, string> Configuration { get; set; } = new();
    
    public List<string> Dependencies { get; set; } = new();
}

/// <summary>
/// Self-evolution configuration
/// </summary>
public class SelfEvolutionConfig : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public bool EnableCodeGeneration { get; set; } = true;
    
    public bool EnableDynamicCompilation { get; set; } = true;
    
    public bool EnablePluginRegistration { get; set; } = true;
    
    public bool EnableSelfModification { get; set; } = false; // Dangerous feature
    
    public int MaxGeneratedFiles { get; set; } = 1000;
    
    public int MaxCompilationTime { get; set; } = 30000; // milliseconds
    
    public List<string> AllowedNamespaces { get; set; } = new();
    
    public List<string> ForbiddenNamespaces { get; set; } = new();
    
    public Dictionary<string, object> AdvancedSettings { get; set; } = new();
}

/// <summary>
/// Code analysis result
/// </summary>
public class CodeAnalysisResult
{
    public bool Success { get; set; }
    
    public List<CodeMetric> Metrics { get; set; } = new();
    
    public List<CodeSuggestion> Suggestions { get; set; } = new();
    
    public List<CodeIssue> Issues { get; set; } = new();
    
    public Dictionary<string, object> AnalysisData { get; set; } = new();
    
    public TimeSpan AnalysisTime { get; set; }
}

/// <summary>
/// Code metric information
/// </summary>
public class CodeMetric
{
    public string Name { get; set; } = string.Empty;
    
    public double Value { get; set; }
    
    public string Unit { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public string Category { get; set; } = string.Empty; // Complexity, Performance, Maintainability, etc.
}

/// <summary>
/// Code improvement suggestion
/// </summary>
public class CodeSuggestion
{
    public string Title { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public string Category { get; set; } = string.Empty; // Performance, Security, Maintainability, etc.
    
    public string Priority { get; set; } = string.Empty; // High, Medium, Low
    
    public string? SuggestedCode { get; set; }
    
    public int? LineNumber { get; set; }
    
    public string? FilePath { get; set; }
}

/// <summary>
/// Code issue/problem
/// </summary>
public class CodeIssue
{
    public string Id { get; set; } = string.Empty;
    
    public string Title { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public string Severity { get; set; } = string.Empty; // Error, Warning, Info
    
    public string Category { get; set; } = string.Empty;
    
    public int? LineNumber { get; set; }
    
    public string? FilePath { get; set; }
    
    public string? SuggestedFix { get; set; }
}

// Strange Loop Models

/// <summary>
/// Request to execute the strange loop of self-evolution
/// </summary>
public class StrangeLoopRequest
{
    public string AgentId { get; set; } = string.Empty;
    
    public string EvolutionType { get; set; } = "SelfModification"; // SelfModification, CapabilityEnhancement, PerformanceOptimization
    
    public Dictionary<string, object> Parameters { get; set; } = new();
    
    public bool EnableHumanApproval { get; set; } = false;
    
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(10);
    
    public int MaxEvolutionSteps { get; set; } = 10;
}

/// <summary>
/// Result of strange loop execution
/// </summary>
public class StrangeLoopResult
{
    public string LoopId { get; set; } = string.Empty;
    
    public bool Success { get; set; }
    
    public string? ErrorMessage { get; set; }
    
    public DateTime StartTime { get; set; }
    
    public DateTime EndTime { get; set; }
    
    public TimeSpan TotalDuration { get; set; }
    
    public List<EvolutionStep> EvolutionSteps { get; set; } = new();
    
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// A single step in the evolution process
/// </summary>
public class EvolutionStep
{
    public string StepName { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public bool Success { get; set; }
    
    public string? ErrorMessage { get; set; }
    
    public DateTime StartTime { get; set; }
    
    public DateTime EndTime { get; set; }
    
    public TimeSpan Duration { get; set; }
    
    public object? Output { get; set; }
    
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Improvement opportunity identified by agent
/// </summary>
public class ImprovementOpportunity
{
    public string Type { get; set; } = string.Empty; // Performance, Capability, AI Enhancement
    
    public string Description { get; set; } = string.Empty;
    
    public string Priority { get; set; } = string.Empty; // High, Medium, Low
    
    public string EstimatedImpact { get; set; } = string.Empty;
    
    public double Confidence { get; set; } = 0.8;
    
    public Dictionary<string, object> Parameters { get; set; } = new();
}

/// <summary>
/// Design for a new capability
/// </summary>
public class CapabilityDesign
{
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public string Type { get; set; } = string.Empty;
    
    public string Priority { get; set; } = string.Empty;
    
    public string SourceCode { get; set; } = string.Empty;
    
    public List<string> Dependencies { get; set; } = new();
    
    public Dictionary<string, string> Configuration { get; set; } = new();
}

/// <summary>
/// Generated capability with compilation result
/// </summary>
public class GeneratedCapability
{
    public string Name { get; set; } = string.Empty;
    
    public string Type { get; set; } = string.Empty;
    
    public string GeneratedCode { get; set; } = string.Empty;
    
    public CompilationResult? CompilationResult { get; set; }
    
    public byte[]? AssemblyBytes { get; set; }
    
    public bool CompilationSuccess { get; set; }
}

/// <summary>
/// Loaded capability in the system
/// </summary>
public class LoadedCapability
{
    public string Name { get; set; } = string.Empty;
    
    public string Type { get; set; } = string.Empty;
    
    public string PluginId { get; set; } = string.Empty;
    
    public bool LoadSuccess { get; set; }
    
    public DateTime LoadTime { get; set; }
    
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Test result for a capability
/// </summary>
public class CapabilityTestResult
{
    public string CapabilityName { get; set; } = string.Empty;
    
    public DateTime TestTime { get; set; }
    
    public bool Success { get; set; }
    
    public Dictionary<string, object> PerformanceMetrics { get; set; } = new();
    
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Reflection on the evolution process
/// </summary>
public class EvolutionReflection
{
    public DateTime EvolutionDate { get; set; }
    
    public int NewCapabilitiesCount { get; set; }
    
    public double SuccessRate { get; set; }
    
    public double PerformanceImprovement { get; set; }
    
    public List<string> FutureImprovements { get; set; } = new();
    
    public string SelfAssessment { get; set; } = string.Empty;
}

/// <summary>
/// Evolution statistics
/// </summary>
public class EvolutionStatistics
{
    public int TotalEvolutions { get; set; }
    
    public int SuccessfulEvolutions { get; set; }
    
    public int FailedEvolutions { get; set; }
    
    public double SuccessRate { get; set; }
    
    public TimeSpan AverageEvolutionTime { get; set; }
    
    public int TotalNewCapabilities { get; set; }
    
    public Dictionary<string, int> EvolutionTypes { get; set; } = new();
    
    public List<string> MostCommonImprovements { get; set; } = new();
} 