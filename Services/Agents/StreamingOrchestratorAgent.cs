using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;
using UltraGenericSystem.Models;

namespace UltraGenericSystem.Services.Agents;

/// <summary>
/// Orchestrator agent that coordinates other agents and manages execution flow
/// </summary>
public class StreamingOrchestratorAgent : StreamingSKAgentBase
{
    public StreamingOrchestratorAgent(Kernel kernel, ILogger logger) 
        : base(kernel, logger, "Orchestrator") { }

    public override async Task<AgentResponse<object>> ExecuteAsync(
        AgentExecutionContext context, 
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            _logger.LogInformation("Orchestrator agent starting execution for {Operation}", context.Operation);
            
            var prompt = BuildPrompt(context);
            var response = await ExecuteChatCompletionAsync(prompt, cancellationToken);
            var result = await ProcessResponseAsync(response, context);
            
            var duration = DateTime.UtcNow - startTime;
            LogExecution(context.Operation, duration, true);
            
            return AgentResponse<object>.Success(result);
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            LogExecution(context.Operation, duration, false);
            _logger.LogError(ex, "Orchestrator agent failed for {Operation}", context.Operation);
            return AgentResponse<object>.Failure($"Orchestrator failed: {ex.Message}");
        }
    }

    public override async IAsyncEnumerable<StreamingChatMessageContent> ExecuteStreamingAsync(
        AgentExecutionContext context,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Orchestrator agent starting streaming execution for {Operation}", context.Operation);
        
        var prompt = BuildPrompt(context);
        
        await foreach (var chunk in ExecuteStreamingChatCompletionAsync(prompt, cancellationToken))
        {
            yield return chunk;
        }
        
        _logger.LogInformation("Orchestrator agent completed streaming execution for {Operation}", context.Operation);
    }

    protected override string BuildPrompt(AgentExecutionContext context)
    {
        return $@"
You are the Orchestrator Agent, responsible for coordinating and managing the execution flow of operations.

Current Operation: {context.Operation}
Entity Type: {context.EntityType?.Name ?? "Unknown"}
Context Data: {context.Data?.ToString() ?? "None"}

Your role is to:
1. Analyze the operation requirements
2. Determine the optimal execution strategy
3. Coordinate with other agents as needed
4. Ensure proper error handling and validation
5. Return a structured response that can be processed by the system

Please provide your orchestration plan and any immediate actions that should be taken.
";
    }

    protected override async Task<object> ProcessResponseAsync(string response, AgentExecutionContext context)
    {
        // Process the orchestrator's response and return structured data
        return new
        {
            OrchestrationPlan = response,
            Operation = context.Operation,
            EntityType = context.EntityType?.Name,
            Timestamp = DateTime.UtcNow,
            Status = "Orchestrated"
        };
    }
} 