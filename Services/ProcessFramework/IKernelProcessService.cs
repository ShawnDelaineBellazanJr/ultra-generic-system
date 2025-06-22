using Microsoft.SemanticKernel;
using UltraGenericSystem.Models;

namespace UltraGenericSystem.Services.ProcessFramework;

/// <summary>
/// Interface for SK Process Framework integration
/// </summary>
public interface IKernelProcessService
{
    /// <summary>
    /// Creates a new process with the specified pattern
    /// </summary>
    Task<ProcessResult> CreateProcessAsync<TInput, TOutput>(
        string processName,
        ProcessPattern pattern,
        TInput input,
        ProcessConfig config,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an Author-Critic process pattern
    /// </summary>
    Task<ProcessResult> ExecuteAuthorCriticAsync<TInput, TOutput>(
        string processName,
        TInput input,
        AuthorCriticConfig config,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a Refine loop process pattern
    /// </summary>
    Task<ProcessResult> ExecuteRefineLoopAsync<TInput, TOutput>(
        string processName,
        TInput input,
        RefineLoopConfig config,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the status of a running process
    /// </summary>
    Task<ProcessStatus> GetProcessStatusAsync(string processId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a running process
    /// </summary>
    Task<bool> CancelProcessAsync(string processId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the Semantic Kernel instance
    /// </summary>
    Kernel GetKernel();
}

/// <summary>
/// Process pattern types
/// </summary>
public enum ProcessPattern
{
    Sequential,
    AuthorCritic,
    RefineLoop,
    Custom
}

/// <summary>
/// Process configuration
/// </summary>
public class ProcessConfig
{
    public int MaxCycles { get; set; } = 10;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(30);
    public bool EnableLogging { get; set; } = true;
    public Dictionary<string, object>? CustomSettings { get; set; }
}

/// <summary>
/// Author-Critic specific configuration
/// </summary>
public class AuthorCriticConfig : ProcessConfig
{
    public string AuthorInstructions { get; set; } = "You are the author responsible for creating content.";
    public string CriticInstructions { get; set; } = "You are the critic responsible for evaluating and providing feedback.";
    public double QualityThreshold { get; set; } = 0.8;
    public int MaxIterations { get; set; } = 5;
}

/// <summary>
/// Refine loop specific configuration
/// </summary>
public class RefineLoopConfig : ProcessConfig
{
    public string RefineInstructions { get; set; } = "You are responsible for refining and improving the output.";
    public Func<object, bool> TerminationCondition { get; set; } = null!;
    public int MaxRefinements { get; set; } = 10;
}

/// <summary>
/// Process result
/// </summary>
public class ProcessResult
{
    public string ProcessId { get; set; } = string.Empty;
    public ProcessStatus Status { get; set; }
    public object? Output { get; set; }
    public List<ProcessStep> Steps { get; set; } = new();
    public TimeSpan Duration { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Process status
/// </summary>
public enum ProcessStatus
{
    Created,
    Running,
    Completed,
    Failed,
    Cancelled
}

/// <summary>
/// Process step information
/// </summary>
public class ProcessStep
{
    public string StepId { get; set; } = string.Empty;
    public string StepName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public ProcessStepStatus Status { get; set; }
    public object? Input { get; set; }
    public object? Output { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Process step status
/// </summary>
public enum ProcessStepStatus
{
    Pending,
    Running,
    Completed,
    Failed,
    Skipped
} 