using Microsoft.SemanticKernel;
using UltraGenericSystem.Models;
namespace UltraGenericSystem.Services;

/// <summary>
/// Interface for advanced agent orchestration with SK Agent Framework
/// </summary>
public interface IAgentOrchestrator
{
    /// <summary>
    /// Executes a single agent operation
    /// </summary>
    Task<AgentResponse<T>> ExecuteAsync<T>(AgentExecutionContext context, CancellationToken cancellationToken = default) where T : UltraGenericSystem.Models.BaseEntity;

    /// <summary>
    /// Executes a query operation with multiple agents
    /// </summary>
    Task<AgentResponse<IEnumerable<T>>> ExecuteQueryAsync<T>(AgentExecutionContext context, CancellationToken cancellationToken = default) where T : UltraGenericSystem.Models.BaseEntity;

    /// <summary>
    /// Executes a delete operation
    /// </summary>
    Task<AgentResponse<bool>> ExecuteDeleteAsync(AgentExecutionContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a multi-agent orchestration pattern
    /// </summary>
    Task<AgentResponse<object>> ExecuteOrchestrationAsync(AgentExecutionContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new agent thread
    /// </summary>
    Task<AgentResponse<string>> CreateThreadAsync(string agentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the Semantic Kernel instance
    /// </summary>
    Kernel GetKernel();

    /// <summary>
    /// Registers a new plugin/skill dynamically
    /// </summary>
    Task<AgentResponse<bool>> RegisterPluginAsync(string pluginName, object pluginInstance, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unregisters a plugin/skill
    /// </summary>
    Task<AgentResponse<bool>> UnregisterPluginAsync(string pluginName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets available plugins/skills
    /// </summary>
    Task<AgentResponse<IEnumerable<string>>> GetAvailablePluginsAsync(CancellationToken cancellationToken = default);

    // Advanced orchestration methods
    Task<AdvancedAgentResponse<TOutput>> ExecuteAdvancedOrchestrationAsync<TInput, TOutput>(
        AdvancedAgentRequest<TInput, TOutput> request);

    Task<StructuredOutput<TOutput>> ExecuteStructuredOrchestrationAsync<TInput, TOutput>(
        StructuredInput<TInput> input,
        AdvancedOrchestrationConfig config)
        where TInput : class
        where TOutput : class;

    Task<WorkflowExecutionResult> ExecuteWorkflowAsync<TInput, TOutput>(
        TInput input,
        List<string> workflowSteps,
        AdvancedOrchestrationConfig config);

    Task<EntityAnalysisResult> AnalyzeEntityAsync<T>(
        T entity,
        string entityType,
        AdvancedOrchestrationConfig config);
} 