using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;
using UltraGenericSystem.Models;

namespace UltraGenericSystem.Services.Agents;

/// <summary>
/// Streaming maker agent for execution and implementation
/// </summary>
public class StreamingMakerAgent : StreamingSKAgentBase
{
    public StreamingMakerAgent(Kernel kernel, ILogger logger) 
        : base(kernel, logger, "Maker") { }
    
    public override async Task<AgentResponse<object>> ExecuteAsync(
        AgentExecutionContext context, 
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            _logger.LogInformation("Maker agent starting execution for {Operation}", context.Operation);
            
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
            _logger.LogError(ex, "Maker agent failed for {Operation}", context.Operation);
            return AgentResponse<object>.Failure($"Maker failed: {ex.Message}");
        }
    }
    
    public override async IAsyncEnumerable<StreamingChatMessageContent> ExecuteStreamingAsync(
        AgentExecutionContext context,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Maker agent starting streaming execution for {Operation}", context.Operation);
        
        var prompt = BuildPrompt(context);
        
        await foreach (var chunk in ExecuteStreamingChatCompletionAsync(prompt, cancellationToken))
        {
            yield return chunk;
        }
        
        _logger.LogInformation("Maker agent completed streaming execution for {Operation}", context.Operation);
    }
    
    protected override string BuildPrompt(AgentExecutionContext context)
    {
        return $@"
You are the Maker Agent, responsible for executing operations and implementing plans.

Current Operation: {context.Operation}
Entity Type: {context.EntityType?.Name ?? "Unknown"}
Context Data: {context.Data?.ToString() ?? "None"}

Your role is to:
1. Execute the planned operation step by step
2. Handle data transformations and business logic
3. Ensure data integrity and consistency
4. Implement error handling and recovery
5. Return the execution results

Please execute the operation and provide:
- Step-by-step execution details
- Results of each step
- Any issues encountered and resolutions
- Final outcome and status
";
    }
    
    protected override async Task<object> ProcessResponseAsync(string response, AgentExecutionContext context)
    {
        // Process the maker's response and return structured data
        return new
        {
            ExecutionResults = response,
            Operation = context.Operation,
            EntityType = context.EntityType?.Name,
            Timestamp = DateTime.UtcNow,
            Status = "Executed"
        };
    }
} 