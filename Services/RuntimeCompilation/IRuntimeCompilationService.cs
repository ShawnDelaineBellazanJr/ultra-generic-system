using Microsoft.CodeAnalysis;
using UltraGenericSystem.Models;

namespace UltraGenericSystem.Services.RuntimeCompilation;

/// <summary>
/// Interface for runtime code compilation using Roslyn
/// </summary>
public interface IRuntimeCompilationService
{
    /// <summary>
    /// Compiles C# code string to an assembly
    /// </summary>
    Task<CompilationResult> CompileCodeAsync(
        string sourceCode,
        string assemblyName,
        CompilationOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Compiles and loads a skill/plugin class
    /// </summary>
    Task<SkillCompilationResult> CompileSkillAsync(
        string skillCode,
        string skillName,
        CompilationOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates code for security and safety
    /// </summary>
    Task<CodeValidationResult> ValidateCodeAsync(
        string sourceCode,
        CodeValidationOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Unloads a compiled assembly
    /// </summary>
    Task<bool> UnloadAssemblyAsync(string assemblyName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets compilation statistics
    /// </summary>
    CompilationStatistics GetStatistics();
}

/// <summary>
/// Compilation options
/// </summary>
public class CompilationOptions
{
    public string TargetFramework { get; set; } = "net9.0";
    public OutputKind OutputKind { get; set; } = OutputKind.DynamicallyLinkedLibrary;
    public OptimizationLevel OptimizationLevel { get; set; } = OptimizationLevel.Release;
    public bool EnableNullableReferenceTypes { get; set; } = true;
    public List<string> AdditionalReferences { get; set; } = new();
    public List<string> UsingStatements { get; set; } = new();
    public Dictionary<string, string> PreprocessorSymbols { get; set; } = new();
    public bool EnableUnsafeCode { get; set; } = false;
    public bool EnableDeterministicCompilation { get; set; } = true;
}

/// <summary>
/// Code validation options
/// </summary>
public class CodeValidationOptions
{
    public bool EnableSecurityValidation { get; set; } = true;
    public bool EnablePerformanceValidation { get; set; } = true;
    public List<string> AllowedNamespaces { get; set; } = new();
    public List<string> BlockedNamespaces { get; set; } = new();
    public List<string> AllowedTypes { get; set; } = new();
    public List<string> BlockedTypes { get; set; } = new();
    public int MaxLinesOfCode { get; set; } = 1000;
    public int MaxComplexity { get; set; } = 10;
    public bool RequireAsyncMethods { get; set; } = false;
    public bool RequireErrorHandling { get; set; } = true;
}

/// <summary>
/// Compilation result
/// </summary>
public class CompilationResult
{
    public bool Success { get; set; }
    public byte[]? AssemblyBytes { get; set; }
    public string? AssemblyPath { get; set; }
    public List<Diagnostic> Diagnostics { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public TimeSpan CompilationTime { get; set; }
    public long AssemblySize { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Skill compilation result
/// </summary>
public class SkillCompilationResult : CompilationResult
{
    public string SkillName { get; set; } = string.Empty;
    public Type? SkillType { get; set; }
    public object? SkillInstance { get; set; }
    public List<string> AvailableFunctions { get; set; } = new();
    public Dictionary<string, string> FunctionDescriptions { get; set; } = new();
}

/// <summary>
/// Code validation result
/// </summary>
public class CodeValidationResult
{
    public bool IsValid { get; set; }
    public List<ValidationIssue> Issues { get; set; } = new();
    public List<string> SecurityWarnings { get; set; } = new();
    public List<string> PerformanceWarnings { get; set; } = new();
    public Dictionary<string, object> Metrics { get; set; } = new();
}

/// <summary>
/// Validation issue
/// </summary>
public class ValidationIssue
{
    public ValidationIssueType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public int LineNumber { get; set; }
    public int ColumnNumber { get; set; }
    public string Code { get; set; } = string.Empty;
    public ValidationSeverity Severity { get; set; }
}

/// <summary>
/// Validation issue type
/// </summary>
public enum ValidationIssueType
{
    Security,
    Performance,
    Style,
    Complexity,
    ErrorHandling,
    AsyncUsage,
    NamespaceUsage,
    TypeUsage
}

/// <summary>
/// Validation severity
/// </summary>
public enum ValidationSeverity
{
    Info,
    Warning,
    Error,
    Critical
}

/// <summary>
/// Compilation statistics
/// </summary>
public class CompilationStatistics
{
    public int TotalCompilations { get; set; }
    public int SuccessfulCompilations { get; set; }
    public int FailedCompilations { get; set; }
    public TimeSpan TotalCompilationTime { get; set; }
    public TimeSpan AverageCompilationTime { get; set; }
    public long TotalAssemblySize { get; set; }
    public int LoadedAssemblies { get; set; }
    public Dictionary<string, int> CompilationErrors { get; set; } = new();
    public Dictionary<string, TimeSpan> PatternCompilationTimes { get; set; } = new();
} 