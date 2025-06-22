using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;
using UltraGenericSystem.Models;

namespace UltraGenericSystem.Services.Agents;

/// <summary>
/// Planner agent that creates execution plans and strategies
/// </summary>
public class StreamingPlannerAgent : StreamingSKAgentBase
{
    public StreamingPlannerAgent(Kernel kernel, ILogger logger) 
        : base(kernel, logger, "Planner") { }

    public override async Task<AgentResponse<object>> ExecuteAsync(
        AgentExecutionContext context, 
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            _logger.LogInformation("Planner agent starting execution for {Operation}", context.Operation);
            
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
            _logger.LogError(ex, "Planner agent failed for {Operation}", context.Operation);
            return AgentResponse<object>.Failure($"Planner failed: {ex.Message}");
        }
    }

    public override async IAsyncEnumerable<StreamingChatMessageContent> ExecuteStreamingAsync(
        AgentExecutionContext context,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Planner agent starting streaming execution for {Operation}", context.Operation);
        
        var prompt = BuildPrompt(context);
        
        await foreach (var chunk in ExecuteStreamingChatCompletionAsync(prompt, cancellationToken))
        {
            yield return chunk;
        }
        
        _logger.LogInformation("Planner agent completed streaming execution for {Operation}", context.Operation);
    }

    protected override string BuildPrompt(AgentExecutionContext context)
    {
        return $@"
You are the Planner Agent, responsible for creating detailed execution plans and strategies.

Current Operation: {context.Operation}
Entity Type: {context.EntityType?.Name ?? "Unknown"}
Context Data: {context.Data?.ToString() ?? "None"}

Your role is to:
1. Analyze the operation requirements
2. Break down the operation into logical steps
3. Identify potential risks and mitigation strategies
4. Define success criteria and validation points
5. Create a detailed execution plan

Please provide a comprehensive plan that includes:
- Step-by-step execution strategy
- Risk assessment and mitigation
- Success criteria
- Resource requirements
- Timeline estimates
";
    }

    protected override async Task<object> ProcessResponseAsync(string response, AgentExecutionContext context)
    {
        // Process the planner's response and return structured data
        return new
        {
            ExecutionPlan = response,
            Operation = context.Operation,
            EntityType = context.EntityType?.Name,
            Timestamp = DateTime.UtcNow,
            Status = "Planned"
        };
    }
} 