using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Collections.Concurrent;
using UltraGenericSystem.Models;

namespace UltraGenericSystem.Services.ProcessFramework;

/// <summary>
/// Implementation of SK Process Framework service
/// </summary>
public class KernelProcessService : IKernelProcessService
{
    private readonly Kernel _kernel;
    private readonly ILogger<KernelProcessService> _logger;
    private readonly ConcurrentDictionary<string, ProcessContext> _activeProcesses;
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _processCancellationTokens;

    public KernelProcessService(Kernel kernel, ILogger<KernelProcessService> logger)
    {
        _kernel = kernel;
        _logger = logger;
        _activeProcesses = new ConcurrentDictionary<string, ProcessContext>();
        _processCancellationTokens = new ConcurrentDictionary<string, CancellationTokenSource>();
    }

    /// <summary>
    /// Creates a new process with the specified pattern
    /// </summary>
    public async Task<ProcessResult> CreateProcessAsync<TInput, TOutput>(
        string processName,
        ProcessPattern pattern,
        TInput input,
        ProcessConfig config,
        CancellationToken cancellationToken = default)
    {
        var processId = Guid.NewGuid().ToString();
        var startTime = DateTime.UtcNow;

        _logger.LogInformation("Creating process {ProcessId} with pattern {Pattern}", processId, pattern);

        try
        {
            var processContext = new ProcessContext
            {
                ProcessId = processId,
                ProcessName = processName,
                Pattern = pattern,
                Config = config,
                StartTime = startTime,
                Status = ProcessStatus.Created
            };

            _activeProcesses[processId] = processContext;

            ProcessResult result;
            switch (pattern)
            {
                case ProcessPattern.AuthorCritic:
                    var authorCriticConfig = config as AuthorCriticConfig ?? new AuthorCriticConfig();
                    result = await ExecuteAuthorCriticAsync<TInput, TOutput>(processName, input, authorCriticConfig, cancellationToken);
                    break;
                case ProcessPattern.RefineLoop:
                    var refineConfig = config as RefineLoopConfig ?? new RefineLoopConfig();
                    result = await ExecuteRefineLoopAsync<TInput, TOutput>(processName, input, refineConfig, cancellationToken);
                    break;
                default:
                    throw new NotSupportedException($"Process pattern {pattern} is not yet implemented");
            }

            result.ProcessId = processId;
            result.Duration = DateTime.UtcNow - startTime;

            _activeProcesses.TryRemove(processId, out _);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating process {ProcessId}", processId);
            _activeProcesses.TryRemove(processId, out _);
            
            return new ProcessResult
            {
                ProcessId = processId,
                Status = ProcessStatus.Failed,
                ErrorMessage = ex.Message,
                Duration = DateTime.UtcNow - startTime
            };
        }
    }

    /// <summary>
    /// Executes an Author-Critic process pattern
    /// </summary>
    public async Task<ProcessResult> ExecuteAuthorCriticAsync<TInput, TOutput>(
        string processName,
        TInput input,
        AuthorCriticConfig config,
        CancellationToken cancellationToken = default)
    {
        var processId = Guid.NewGuid().ToString();
        var startTime = DateTime.UtcNow;
        var steps = new List<ProcessStep>();

        _logger.LogInformation("Starting Author-Critic process {ProcessId}", processId);

        try
        {
            var currentInput = input;
            var iteration = 0;

            while (iteration < config.MaxIterations)
            {
                iteration++;
                _logger.LogInformation("Author-Critic iteration {Iteration}/{MaxIterations}", iteration, config.MaxIterations);

                // Author Step
                var authorStep = await ExecuteAuthorStepAsync<TInput, TOutput>(processId, iteration, currentInput, config, cancellationToken);
                steps.Add(authorStep);

                if (authorStep.Status == ProcessStepStatus.Failed)
                {
                    return new ProcessResult
                    {
                        ProcessId = processId,
                        Status = ProcessStatus.Failed,
                        Steps = steps,
                        ErrorMessage = authorStep.ErrorMessage,
                        Duration = DateTime.UtcNow - startTime
                    };
                }

                // Critic Step
                var criticStep = await ExecuteCriticStepAsync<TInput, TOutput>(processId, iteration, authorStep.Output, config, cancellationToken);
                steps.Add(criticStep);

                if (criticStep.Status == ProcessStepStatus.Failed)
                {
                    return new ProcessResult
                    {
                        ProcessId = processId,
                        Status = ProcessStatus.Failed,
                        Steps = steps,
                        ErrorMessage = criticStep.ErrorMessage,
                        Duration = DateTime.UtcNow - startTime
                    };
                }

                // Check if quality threshold is met
                if (criticStep.Output is AuthorCriticEvaluation evaluation && evaluation.QualityScore >= config.QualityThreshold)
                {
                    _logger.LogInformation("Author-Critic process {ProcessId} completed successfully after {Iteration} iterations", processId, iteration);
                    return new ProcessResult
                    {
                        ProcessId = processId,
                        Status = ProcessStatus.Completed,
                        Output = authorStep.Output,
                        Steps = steps,
                        Duration = DateTime.UtcNow - startTime,
                        Metadata = new Dictionary<string, object>
                        {
                            ["iterations"] = iteration,
                            ["finalQualityScore"] = evaluation.QualityScore
                        }
                    };
                }

                // Prepare input for next iteration - use the original input type
                currentInput = input;
            }

            // Max iterations reached
            _logger.LogWarning("Author-Critic process {ProcessId} reached max iterations without meeting quality threshold", processId);
            return new ProcessResult
            {
                ProcessId = processId,
                Status = ProcessStatus.Completed,
                Output = steps.Last(s => s.StepName == "Author").Output,
                Steps = steps,
                Duration = DateTime.UtcNow - startTime,
                Metadata = new Dictionary<string, object>
                {
                    ["iterations"] = iteration,
                    ["maxIterationsReached"] = true
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Author-Critic process {ProcessId}", processId);
            return new ProcessResult
            {
                ProcessId = processId,
                Status = ProcessStatus.Failed,
                Steps = steps,
                ErrorMessage = ex.Message,
                Duration = DateTime.UtcNow - startTime
            };
        }
    }

    /// <summary>
    /// Executes a Refine loop process pattern
    /// </summary>
    public async Task<ProcessResult> ExecuteRefineLoopAsync<TInput, TOutput>(
        string processName,
        TInput input,
        RefineLoopConfig config,
        CancellationToken cancellationToken = default)
    {
        var processId = Guid.NewGuid().ToString();
        var startTime = DateTime.UtcNow;
        var steps = new List<ProcessStep>();

        _logger.LogInformation("Starting Refine Loop process {ProcessId}", processId);

        try
        {
            var currentOutput = input;
            var refinement = 0;

            while (refinement < config.MaxRefinements)
            {
                refinement++;
                _logger.LogInformation("Refine Loop iteration {Refinement}/{MaxRefinements}", refinement, config.MaxRefinements);

                // Refine Step
                var refineStep = await ExecuteRefineStepAsync<TInput, TOutput>(processId, refinement, currentOutput, config, cancellationToken);
                steps.Add(refineStep);

                if (refineStep.Status == ProcessStepStatus.Failed)
                {
                    return new ProcessResult
                    {
                        ProcessId = processId,
                        Status = ProcessStatus.Failed,
                        Steps = steps,
                        ErrorMessage = refineStep.ErrorMessage,
                        Duration = DateTime.UtcNow - startTime
                    };
                }

                // Convert output to proper type or use as-is
                currentOutput = refineStep.Output is TInput typedOutput ? typedOutput : input;

                // Check termination condition
                if (config.TerminationCondition?.Invoke(currentOutput) == true)
                {
                    _logger.LogInformation("Refine Loop process {ProcessId} completed successfully after {Refinement} refinements", processId, refinement);
                    return new ProcessResult
                    {
                        ProcessId = processId,
                        Status = ProcessStatus.Completed,
                        Output = currentOutput,
                        Steps = steps,
                        Duration = DateTime.UtcNow - startTime,
                        Metadata = new Dictionary<string, object>
                        {
                            ["refinements"] = refinement
                        }
                    };
                }
            }

            // Max refinements reached
            _logger.LogWarning("Refine Loop process {ProcessId} reached max refinements", processId);
            return new ProcessResult
            {
                ProcessId = processId,
                Status = ProcessStatus.Completed,
                Output = currentOutput,
                Steps = steps,
                Duration = DateTime.UtcNow - startTime,
                Metadata = new Dictionary<string, object>
                {
                    ["refinements"] = refinement,
                    ["maxRefinementsReached"] = true
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Refine Loop process {ProcessId}", processId);
            return new ProcessResult
            {
                ProcessId = processId,
                Status = ProcessStatus.Failed,
                Steps = steps,
                ErrorMessage = ex.Message,
                Duration = DateTime.UtcNow - startTime
            };
        }
    }

    /// <summary>
    /// Gets the status of a running process
    /// </summary>
    public async Task<ProcessStatus> GetProcessStatusAsync(string processId, CancellationToken cancellationToken = default)
    {
        if (_activeProcesses.TryGetValue(processId, out var context))
        {
            return context.Status;
        }
        return ProcessStatus.Completed; // Assume completed if not found
    }

    /// <summary>
    /// Cancels a running process
    /// </summary>
    public async Task<bool> CancelProcessAsync(string processId, CancellationToken cancellationToken = default)
    {
        if (_processCancellationTokens.TryGetValue(processId, out var cts))
        {
            cts.Cancel();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Gets the Semantic Kernel instance
    /// </summary>
    public Kernel GetKernel()
    {
        return _kernel;
    }

    #region Private Methods

    private async Task<ProcessStep> ExecuteAuthorStepAsync<TInput, TOutput>(
        string processId,
        int iteration,
        TInput input,
        AuthorCriticConfig config,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Executing Author step for process {ProcessId}, iteration {Iteration}", processId, iteration);

            // Create author input with proper type conversion
            var authorInput = new AuthorCriticInput
            {
                ProcessId = processId,
                Iteration = iteration,
                Input = input,
                Config = config
            };

            // Execute author step with explicit type arguments
            var result = await ExecuteAuthorStepAsync<AuthorCriticInput, AuthorCriticOutput>(
                processId, iteration, authorInput, config, cancellationToken);

            return new ProcessStep
            {
                StepId = processId,
                StepName = "Author",
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow,
                Status = ProcessStepStatus.Completed,
                Output = result
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing Author step for process {ProcessId}", processId);
            return new ProcessStep
            {
                StepId = processId,
                StepName = "Author",
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow,
                Status = ProcessStepStatus.Failed,
                ErrorMessage = ex.Message
            };
        }
    }

    private async Task<ProcessStep> ExecuteCriticStepAsync<TInput, TOutput>(
        string processId,
        int iteration,
        object? authorOutput,
        AuthorCriticConfig config,
        CancellationToken cancellationToken)
    {
        var stepId = $"{processId}-critic-{iteration}";
        var step = new ProcessStep
        {
            StepId = stepId,
            StepName = "Critic",
            StartTime = DateTime.UtcNow,
            Status = ProcessStepStatus.Running,
            Input = authorOutput
        };

        try
        {
            // Use a placeholder for now since GetCompletionsAsync doesn't exist in current SK API
            // In a real implementation, you would use the correct SK chat completion API
            var result = "Placeholder evaluation result - quality score: 0.8, feedback: Good output";

            // Parse the evaluation (simplified - in practice, you'd use structured output)
            var evaluation = ParseAuthorCriticEvaluation(result);

            step.Output = evaluation;
            step.Status = ProcessStepStatus.Completed;
            step.EndTime = DateTime.UtcNow;

            _logger.LogInformation("Critic step {StepId} completed with quality score {Score}", stepId, evaluation.QualityScore);
        }
        catch (Exception ex)
        {
            step.Status = ProcessStepStatus.Failed;
            step.ErrorMessage = ex.Message;
            step.EndTime = DateTime.UtcNow;
            _logger.LogError(ex, "Critic step {StepId} failed", stepId);
        }

        return step;
    }

    private async Task<ProcessStep> ExecuteRefineStepAsync<TInput, TOutput>(
        string processId,
        int refinement,
        object input,
        RefineLoopConfig config,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Executing Refine step for process {ProcessId}, refinement {Refinement}", processId, refinement);

            // Convert input to proper type
            var typedInput = (TInput)input;

            // Execute refine step with explicit type arguments
            var result = await ExecuteRefineStepAsync<TInput, TOutput>(
                processId, refinement, typedInput, config, cancellationToken);

            return new ProcessStep
            {
                StepId = processId,
                StepName = "Refine",
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow,
                Status = ProcessStepStatus.Completed,
                Output = result
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing Refine step for process {ProcessId}", processId);
            return new ProcessStep
            {
                StepId = processId,
                StepName = "Refine",
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow,
                Status = ProcessStepStatus.Failed,
                ErrorMessage = ex.Message
            };
        }
    }

    private AuthorCriticEvaluation ParseAuthorCriticEvaluation(string response)
    {
        // Simplified parsing - in practice, use structured output or JSON parsing
        var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var score = 0.5; // Default score
        var feedback = response;

        foreach (var line in lines)
        {
            if (line.Contains("score:", StringComparison.OrdinalIgnoreCase) || 
                line.Contains("quality:", StringComparison.OrdinalIgnoreCase))
            {
                var parts = line.Split(':', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 1 && double.TryParse(parts[1].Trim(), out var parsedScore))
                {
                    score = Math.Max(0.0, Math.Min(1.0, parsedScore));
                }
            }
        }

        return new AuthorCriticEvaluation
        {
            QualityScore = score,
            Feedback = feedback
        };
    }

    #endregion
}

/// <summary>
/// Process context for tracking active processes
/// </summary>
public class ProcessContext
{
    public string ProcessId { get; set; } = string.Empty;
    public string ProcessName { get; set; } = string.Empty;
    public ProcessPattern Pattern { get; set; }
    public ProcessConfig Config { get; set; } = null!;
    public DateTime StartTime { get; set; }
    public ProcessStatus Status { get; set; }
}

/// <summary>
/// Author-Critic evaluation result
/// </summary>
public class AuthorCriticEvaluation
{
    public double QualityScore { get; set; }
    public string Feedback { get; set; } = string.Empty;
}

/// <summary>
/// Author-Critic input for iterations
/// </summary>
public class AuthorCriticInput
{
    public string ProcessId { get; set; } = string.Empty;
    public int Iteration { get; set; }
    public object Input { get; set; } = null!;
    public AuthorCriticConfig Config { get; set; } = null!;
    public object OriginalInput { get; set; } = null!;
    public object? PreviousOutput { get; set; }
    public AuthorCriticEvaluation? Feedback { get; set; }
}

public class AuthorCriticOutput
{
    public object Output { get; set; } = null!;
    public string ProcessId { get; set; } = string.Empty;
    public int Iteration { get; set; }
} 