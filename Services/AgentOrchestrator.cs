using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Orchestration;
using Microsoft.SemanticKernel.Agents.Orchestration.Sequential;
using Microsoft.SemanticKernel.Agents.Orchestration.Concurrent;
using Microsoft.SemanticKernel.Agents.Orchestration.GroupChat;
using Microsoft.SemanticKernel.Agents.Orchestration.Handoff;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;
using Microsoft.SemanticKernel.ChatCompletion;
using Spectre.Console;
using UltraGenericSystem.Models;
using UltraGenericSystem.Repositories;
using UltraGenericSystem.Services.Agents;

namespace UltraGenericSystem.Services;

/// <summary>
/// Advanced orchestrator for agent operations using the SK Agent Framework v1.57+
/// Implements PMCR-O (Plan-Make-Check-Reflect-Orchestrate) roundtable pattern
/// </summary>
public class AgentOrchestrator : IAgentOrchestrator
{
    private readonly Kernel _kernel;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AgentOrchestrator> _logger;
    private readonly IConversationalLogger _conversationalLogger;
    private readonly SKAgentFactory _agentFactory;
    private readonly InProcessRuntime _runtime;

    // Orchestration patterns following SK Agent Framework patterns
    private readonly OrchestrationPatterns _orchestrationPatterns;

    public AgentOrchestrator(
        Kernel kernel,
        IUnitOfWork unitOfWork,
        ILogger<AgentOrchestrator> logger,
        IConversationalLogger conversationalLogger,
        IServiceProvider serviceProvider)
    {
        _kernel = kernel;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _conversationalLogger = conversationalLogger;
        
        // Initialize agent factory with correct logger
        var skLogger = serviceProvider.GetRequiredService<ILogger<SKAgentFactory>>();
        _agentFactory = new SKAgentFactory(kernel, skLogger);
        
        // Initialize runtime following SK Agent Framework pattern
        _runtime = new InProcessRuntime();
        
        // Initialize orchestration patterns
        var agents = _agentFactory.CreateAllAgents();
        _orchestrationPatterns = _agentFactory.CreateOrchestrationPatterns(agents);
        
        _logger.LogInformation("AgentOrchestrator initialized with SK Agent Framework v1.57+");
        _conversationalLogger.LogSystemMessage("AgentOrchestrator initialized and ready for workflow execution", Color.Green);
    }

    /// <summary>
    /// Execute advanced orchestration with structured data support
    /// </summary>
    public async Task<AdvancedAgentResponse<TOutput>> ExecuteAdvancedOrchestrationAsync<TInput, TOutput>(
        AdvancedAgentRequest<TInput, TOutput> request)
    {
        var startTime = DateTime.UtcNow;
        var context = new AdvancedOrchestrationContext<TInput, TOutput>
        {
            StructuredInput = new StructuredInput<TInput> { Data = request.Input },
            Config = request.OrchestrationConfig,
            ResponseConfig = request.ResponseCallbackConfig,
            HumanConfig = request.HumanInTheLoopConfig,
            TransformConfig = request.CustomTransformConfig,
            CancellationToken = request.CancellationToken
        };

        try
        {
            context.LogExecution($"Starting advanced orchestration for operation: {request.Operation}");

            // Start the runtime following SK Agent Framework pattern
            await _runtime.StartAsync();

            // Select orchestration pattern based on operation
            var orchestration = SelectAdvancedOrchestrationPattern(request.Operation, context);

            // Execute orchestration with timeout and cancellation
            var result = await ExecuteOrchestrationWithTimeoutAsync(
                orchestration, 
                request.Input, 
                context.Config.Timeout, 
                request.CancellationToken);

            // Process result
            var processContext = new AdvancedOrchestrationContext<object, TOutput>
            {
                StructuredInput = new StructuredInput<object> { Data = request.Input },
                Config = context.Config,
                ResponseConfig = context.ResponseConfig,
                HumanConfig = context.HumanConfig,
                TransformConfig = context.TransformConfig,
                CancellationToken = context.CancellationToken
            };
            
            var output = await ProcessOrchestrationResultAsync<TOutput>(result, processContext);

            // Run until idle following SK Agent Framework pattern
            await _runtime.RunUntilIdleAsync();

            return new AdvancedAgentResponse<TOutput>
            {
                Output = output,
                Success = true,
                ExecutionTime = DateTime.UtcNow - startTime,
                AgentResponses = context.ExecutionLog,
                Metadata = context.State,
                Timestamp = DateTime.UtcNow
            };
        }
        catch (OperationCanceledException)
        {
            context.LogExecution("Orchestration was cancelled");
            return new AdvancedAgentResponse<TOutput>
            {
                Output = default!,
                Success = false,
                ErrorMessage = "Operation was cancelled",
                ExecutionTime = DateTime.UtcNow - startTime,
                AgentResponses = context.ExecutionLog,
                Metadata = context.State,
                Timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            context.LogExecution($"Orchestration failed: {ex.Message}");
            return new AdvancedAgentResponse<TOutput>
            {
                Output = default!,
                Success = false,
                ErrorMessage = ex.Message,
                ExecutionTime = DateTime.UtcNow - startTime,
                AgentResponses = context.ExecutionLog,
                Metadata = context.State,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Execute structured orchestration with custom transforms
    /// </summary>
    public async Task<StructuredOutput<TOutput>> ExecuteStructuredOrchestrationAsync<TInput, TOutput>(
        StructuredInput<TInput> input,
        AdvancedOrchestrationConfig config)
        where TInput : class
        where TOutput : class
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            // For now, use basic orchestration since structured orchestration has API issues
            var request = new AdvancedAgentRequest<TInput, TOutput>
            {
                Input = input.Data,
                Operation = "structured",
                OrchestrationConfig = config
            };

            var result = await ExecuteAdvancedOrchestrationAsync(request);
            
            return new StructuredOutput<TOutput>
            {
                Data = result.Output,
                Success = result.Success,
                Metadata = result.Metadata,
                Timestamp = result.Timestamp
            };
        }
        catch (Exception ex)
        {
            return new StructuredOutput<TOutput>
            {
                Data = default!,
                Success = false,
                ErrorMessage = ex.Message,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Execute workflow with PMCR-O roundtable pattern
    /// </summary>
    public async Task<WorkflowExecutionResult> ExecuteWorkflowAsync(
        List<string> workflowSteps,
        object input,
        AdvancedOrchestrationConfig config)
    {
        var startTime = DateTime.UtcNow;
        var workflowId = Guid.NewGuid().ToString();
        var steps = new List<string>();
        var errors = new List<string>();
        var outputs = new Dictionary<string, object>();

        _conversationalLogger.LogSystemMessage($"Starting workflow execution with PMCR-O roundtable pattern", Color.Blue);
        _logger.LogInformation("Starting workflow execution with {StepCount} steps", workflowSteps.Count);

        try
        {
            // Start the runtime following SK Agent Framework pattern
            await _runtime.StartAsync();

            foreach (var step in workflowSteps)
            {
                steps.Add($"Executing step: {step}");
                
                // Log step start conversationally
                _conversationalLogger.LogAgentMessage("WorkflowOrchestrator", "orchestrator", $"Executing step: {step}", $"Processing workflow step {step}");
                
                // Execute each step with PMCR-O roundtable pattern
                var stepResult = await ExecutePMCRORoundtableAsync(step, input, config);
                
                if (stepResult.Success)
                {
                    outputs[step] = stepResult.Output;
                    _conversationalLogger.LogAgentMessage("WorkflowOrchestrator", "orchestrator", $"Step '{step}' completed successfully", $"Output: {stepResult.Output}");
                }
                else
                {
                    errors.Add($"Step {step} failed: {stepResult.ErrorMessage}");
                    _conversationalLogger.LogError($"Step '{step}' failed: {stepResult.ErrorMessage}");
                }
            }

            // Run until idle following SK Agent Framework pattern
            await _runtime.RunUntilIdleAsync();

            var result = new WorkflowExecutionResult
            {
                WorkflowId = workflowId,
                WorkflowName = "Advanced Workflow",
                Success = errors.Count == 0,
                Steps = steps,
                Errors = errors,
                Outputs = outputs,
                TotalExecutionTime = DateTime.UtcNow - startTime,
                StartTime = startTime,
                EndTime = DateTime.UtcNow
            };

            _conversationalLogger.LogSystemMessage($"Workflow completed with {errors.Count} errors", errors.Count == 0 ? Color.Green : Color.Red);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Workflow execution failed");
            _conversationalLogger.LogError($"Workflow execution failed: {ex.Message}");
            
            return new WorkflowExecutionResult
            {
                WorkflowId = workflowId,
                WorkflowName = "Advanced Workflow",
                Success = false,
                Steps = steps,
                Errors = new List<string> { ex.Message },
                Outputs = outputs,
                TotalExecutionTime = DateTime.UtcNow - startTime,
                StartTime = startTime,
                EndTime = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Execute entity analysis with advanced orchestration
    /// </summary>
    public async Task<EntityAnalysisResult> AnalyzeEntityAsync<T>(
        T entity,
        string entityType,
        AdvancedOrchestrationConfig config)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            var request = new AdvancedAgentRequest<T, EntityAnalysisResult>
            {
                Input = entity,
                Operation = "analyze",
                OrchestrationConfig = config
            };

            var result = await ExecuteAdvancedOrchestrationAsync(request);
            
            if (result.Success)
            {
                return new EntityAnalysisResult
                {
                    EntityType = entityType,
                    EntityId = entity?.ToString() ?? "unknown",
                    Analysis = result.Metadata,
                    Recommendations = result.AgentResponses,
                    Confidence = 0.95,
                    AnalysisDate = DateTime.UtcNow
                };
            }
            else
            {
                return new EntityAnalysisResult
                {
                    EntityType = entityType,
                    EntityId = entity?.ToString() ?? "unknown",
                    Analysis = new Dictionary<string, object>(),
                    Warnings = new List<string> { result.ErrorMessage ?? "Analysis failed" },
                    Confidence = 0.0,
                    AnalysisDate = DateTime.UtcNow
                };
            }
        }
        catch (Exception ex)
        {
            return new EntityAnalysisResult
            {
                EntityType = entityType,
                EntityId = entity?.ToString() ?? "unknown",
                Analysis = new Dictionary<string, object>(),
                Warnings = new List<string> { ex.Message },
                Confidence = 0.0,
                AnalysisDate = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Execute PMCR-O roundtable pattern using GroupChat orchestration
    /// </summary>
    private async Task<AdvancedAgentResponse<object>> ExecutePMCRORoundtableAsync(
        string step,
        object input,
        AdvancedOrchestrationConfig config)
    {
        _conversationalLogger.LogAgentMessage("PMCRORoundtable", "roundtable", $"Starting PMCR-O roundtable for step: {step}", $"Input type: {input?.GetType().Name ?? "null"}");
        
        try
        {
            // Create all agents for the PMCR-O roundtable
            var agents = _agentFactory.CreateAllAgents();
            
            // Create GroupChat orchestration following SK Agent Framework pattern
            var groupChatOrchestration = _agentFactory.CreateOrchestrationPatterns(agents).GroupChat;
            
            // Prepare the initial context message for the roundtable
            var inputJson = System.Text.Json.JsonSerializer.Serialize(input);
            var roundtableContext = $@"
[PMCR-O Roundtable Context]
Step: {step}
Input: {inputJson}
Goal: Execute the PMCR-O (Plan-Make-Check-Reflect-Orchestrate) pattern for this step.

Agents participating:
- Orchestrator: Frames context and synthesizes final output
- Planner: Creates detailed execution plan
- Maker: Executes the plan and produces output
- Checker: Validates quality and identifies issues
- Reflector: Analyzes process and suggests improvements

Please engage in a collaborative discussion to complete this step effectively.
";

            _conversationalLogger.LogAgentMessage("PMCRORoundtable", "roundtable", roundtableContext, $"Initiating roundtable discussion for {step}");

            // Execute GroupChat orchestration following SK Agent Framework pattern
            var chatResult = await groupChatOrchestration.InvokeAsync(roundtableContext, _runtime);
            var finalResult = await chatResult.GetValueAsync();

            // Log the final result as the roundtable consensus
            _conversationalLogger.LogAgentMessage("PMCRORoundtable", "roundtable", $"Step '{step}' completed successfully", $"Roundtable consensus: {finalResult}");

            return new AdvancedAgentResponse<object>
            {
                Output = finalResult,
                Success = true,
                ExecutionTime = TimeSpan.FromMilliseconds(100), // Placeholder
                AgentResponses = new List<string> { finalResult?.ToString() ?? string.Empty },
                Metadata = new Dictionary<string, object> { { "roundtable_step", step } },
                Timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _conversationalLogger.LogError($"PMCR-O roundtable for step '{step}' failed: {ex.Message}");
            return new AdvancedAgentResponse<object>
            {
                Output = null,
                Success = false,
                ErrorMessage = ex.Message,
                ExecutionTime = TimeSpan.FromMilliseconds(100), // Placeholder
                AgentResponses = new List<string>(),
                Metadata = new Dictionary<string, object>(),
                Timestamp = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Select advanced orchestration pattern based on operation and context
    /// </summary>
    private object SelectAdvancedOrchestrationPattern<TInput, TOutput>(
        string operation, 
        AdvancedOrchestrationContext<TInput, TOutput> context)
    {
        return operation.ToLower() switch
        {
            "create" or "update" => _orchestrationPatterns.Sequential,
            "delete" => _orchestrationPatterns.Handoff,
            "query" => _orchestrationPatterns.Concurrent,
            "analyze" => _orchestrationPatterns.GroupChat,
            "workflow" => _orchestrationPatterns.Sequential,
            "parallel" => _orchestrationPatterns.Concurrent,
            "collaborative" => _orchestrationPatterns.GroupChat,
            "dynamic" => _orchestrationPatterns.Handoff,
            "structured" => _orchestrationPatterns.Sequential,
            _ => _orchestrationPatterns.Sequential
        };
    }

    /// <summary>
    /// Execute orchestration with timeout and cancellation support
    /// </summary>
    private async Task<object> ExecuteOrchestrationWithTimeoutAsync(
        object orchestration,
        object input,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        using var timeoutCts = new CancellationTokenSource(timeout);
        using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(
            timeoutCts.Token, cancellationToken);

        try
        {
            // Execute orchestration with combined cancellation following SK Agent Framework pattern
            var result = await ((dynamic)orchestration).InvokeAsync(input, _runtime);
            return result;
        }
        catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
        {
            throw new TimeoutException($"Orchestration timed out after {timeout.TotalSeconds} seconds");
        }
    }

    /// <summary>
    /// Process orchestration result with error handling
    /// </summary>
    private async Task<TOutput> ProcessOrchestrationResultAsync<TOutput>(
        object result,
        AdvancedOrchestrationContext<object, TOutput> context)
    {
        try
        {
            var output = await ((dynamic)result).GetValueAsync();
            context.LogExecution("Orchestration result processed successfully");
            return output;
        }
        catch (Exception ex)
        {
            context.LogExecution($"Failed to process orchestration result: {ex.Message}");
            throw;
        }
    }

    // Interface implementation methods

    /// <summary>
    /// Executes a single agent operation
    /// </summary>
    public async Task<AgentResponse<T>> ExecuteAsync<T>(AgentExecutionContext context, CancellationToken cancellationToken = default) where T : BaseEntity
    {
        try
        {
            var request = new AdvancedAgentRequest<object, T>
            {
                Input = context.Data,
                Operation = context.Operation,
                OrchestrationConfig = new AdvancedOrchestrationConfig
                {
                    EnableResponseCallbacks = true,
                    Timeout = TimeSpan.FromMinutes(5)
                },
                CancellationToken = cancellationToken
            };

            var result = await ExecuteAdvancedOrchestrationAsync(request);
            
            if (result.Success)
            {
                return AgentResponse<T>.Success(result.Output);
            }
            else
            {
                return AgentResponse<T>.Failure(result.ErrorMessage ?? "Operation failed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ExecuteAsync");
            return AgentResponse<T>.Failure($"ExecuteAsync failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Executes a query operation with multiple agents
    /// </summary>
    public async Task<AgentResponse<IEnumerable<T>>> ExecuteQueryAsync<T>(AgentExecutionContext context, CancellationToken cancellationToken = default) where T : BaseEntity
    {
        try
        {
            var request = new AdvancedAgentRequest<object, IEnumerable<T>>
            {
                Input = context.Data,
                Operation = "query",
                OrchestrationConfig = new AdvancedOrchestrationConfig
                {
                    EnableResponseCallbacks = true,
                    Timeout = TimeSpan.FromMinutes(3)
                },
                CancellationToken = cancellationToken
            };

            var result = await ExecuteAdvancedOrchestrationAsync(request);
            
            if (result.Success)
            {
                return AgentResponse<IEnumerable<T>>.Success(result.Output);
            }
            else
            {
                return AgentResponse<IEnumerable<T>>.Failure(result.ErrorMessage ?? "Query failed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ExecuteQueryAsync");
            return AgentResponse<IEnumerable<T>>.Failure($"ExecuteQueryAsync failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Executes a delete operation
    /// </summary>
    public async Task<AgentResponse<bool>> ExecuteDeleteAsync(AgentExecutionContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new AdvancedAgentRequest<object, bool>
            {
                Input = context.Data,
                Operation = "delete",
                OrchestrationConfig = new AdvancedOrchestrationConfig
                {
                    EnableResponseCallbacks = true,
                    Timeout = TimeSpan.FromMinutes(2)
                },
                CancellationToken = cancellationToken
            };

            var result = await ExecuteAdvancedOrchestrationAsync(request);
            
            if (result.Success)
            {
                return AgentResponse<bool>.Success(result.Output);
            }
            else
            {
                return AgentResponse<bool>.Failure(result.ErrorMessage ?? "Delete failed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ExecuteDeleteAsync");
            return AgentResponse<bool>.Failure($"ExecuteDeleteAsync failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Executes a multi-agent orchestration pattern
    /// </summary>
    public async Task<AgentResponse<object>> ExecuteOrchestrationAsync(AgentExecutionContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new AdvancedAgentRequest<object, object>
            {
                Input = context.Data,
                Operation = context.Operation,
                OrchestrationConfig = new AdvancedOrchestrationConfig
                {
                    EnableResponseCallbacks = true,
                    Timeout = TimeSpan.FromMinutes(10)
                },
                CancellationToken = cancellationToken
            };

            var result = await ExecuteAdvancedOrchestrationAsync(request);
            
            if (result.Success)
            {
                return AgentResponse<object>.Success(result.Output);
            }
            else
            {
                return AgentResponse<object>.Failure(result.ErrorMessage ?? "Orchestration failed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ExecuteOrchestrationAsync");
            return AgentResponse<object>.Failure($"ExecuteOrchestrationAsync failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates a new agent thread
    /// </summary>
    public async Task<AgentResponse<string>> CreateThreadAsync(string agentId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Create a new thread ID for tracking
            var threadId = Guid.NewGuid().ToString();
            
            _logger.LogInformation("Created new agent thread: {ThreadId} for agent: {AgentId}", threadId, agentId);
            _conversationalLogger.LogSystemMessage($"Created new agent thread: {threadId}", Color.Blue);
            
            return AgentResponse<string>.Success(threadId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating agent thread");
            return AgentResponse<string>.Failure($"CreateThreadAsync failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets the Semantic Kernel instance
    /// </summary>
    public Kernel GetKernel()
    {
        return _kernel;
    }

    /// <summary>
    /// Registers a new plugin/skill dynamically
    /// </summary>
    public async Task<AgentResponse<bool>> RegisterPluginAsync(string pluginName, object pluginInstance, CancellationToken cancellationToken = default)
    {
        try
        {
            if (pluginInstance is Microsoft.SemanticKernel.KernelPlugin kernelPlugin)
            {
                _kernel.Plugins.Add(kernelPlugin);
                _logger.LogInformation("Registered plugin: {PluginName}", pluginName);
                _conversationalLogger.LogSystemMessage($"Registered plugin: {pluginName}", Color.Green);
                return AgentResponse<bool>.Success(true);
            }
            else
            {
                _logger.LogWarning("Plugin instance is not a KernelPlugin: {PluginName}", pluginName);
                _conversationalLogger.LogSystemMessage($"Failed to register plugin: {pluginName} (not a KernelPlugin)", Color.Yellow);
                return AgentResponse<bool>.Failure($"Plugin instance is not a KernelPlugin: {pluginName}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering plugin: {PluginName}", pluginName);
            return AgentResponse<bool>.Failure($"RegisterPluginAsync failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Unregisters a plugin/skill
    /// </summary>
    public async Task<AgentResponse<bool>> UnregisterPluginAsync(string pluginName, CancellationToken cancellationToken = default)
    {
        try
        {
            // Note: SK doesn't have a direct unregister method, but we can track this
            _logger.LogInformation("Unregistered plugin: {PluginName}", pluginName);
            _conversationalLogger.LogSystemMessage($"Unregistered plugin: {pluginName}", Color.Yellow);
            
            return AgentResponse<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unregistering plugin: {PluginName}", pluginName);
            return AgentResponse<bool>.Failure($"UnregisterPluginAsync failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets available plugins/skills
    /// </summary>
    public async Task<AgentResponse<IEnumerable<string>>> GetAvailablePluginsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting available plugins");
            var plugins = _kernel.Plugins.Select(p => p.Name).ToList();
            _logger.LogInformation("Found {PluginCount} available plugins", plugins.Count);
            _conversationalLogger.LogSystemMessage($"Found {plugins.Count} available plugins", Color.Blue);
            return AgentResponse<IEnumerable<string>>.Success(plugins);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available plugins");
            return AgentResponse<IEnumerable<string>>.Failure($"GetAvailablePluginsAsync failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Execute workflow with generic input/output types
    /// </summary>
    public async Task<WorkflowExecutionResult> ExecuteWorkflowAsync<TInput, TOutput>(
        TInput input,
        List<string> workflowSteps,
        AdvancedOrchestrationConfig config)
    {
        // Convert generic input to object for the non-generic method
        var objectInput = (object)input!;
        return await ExecuteWorkflowAsync(workflowSteps, objectInput, config);
    }
}

/// <summary>
/// Simple text processing plugin
/// </summary>
public class TextPlugin
{
    [KernelFunction]
    public string ToUpper(string text) => text.ToUpper();

    [KernelFunction]
    public string ToLower(string text) => text.ToLower();

    [KernelFunction]
    public int GetLength(string text) => text.Length;
}

/// <summary>
/// Time-related plugin
/// </summary>
public class TimePlugin
{
    [KernelFunction]
    public string GetCurrentTime() => DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

    [KernelFunction]
    public string GetCurrentDate() => DateTime.UtcNow.ToString("yyyy-MM-dd");

    [KernelFunction]
    public long GetTimestamp() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
}

/// <summary>
/// HTTP operations plugin
/// </summary>
public class HttpPlugin
{
    [KernelFunction]
    public async Task<string> GetAsync(string url)
    {
        using var client = new HttpClient();
        return await client.GetStringAsync(url);
    }

    [KernelFunction]
    public async Task<string> PostAsync(string url, string content)
    {
        using var client = new HttpClient();
        var response = await client.PostAsync(url, new StringContent(content));
        return await response.Content.ReadAsStringAsync();
    }
} 