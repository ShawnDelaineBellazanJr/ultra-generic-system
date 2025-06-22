using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;
using UltraGenericSystem.Models;

namespace UltraGenericSystem.Services.Agents;

/// <summary>
/// Checker agent that validates operations and ensures quality
/// </summary>
public class StreamingCheckerAgent : StreamingSKAgentBase
{
    public StreamingCheckerAgent(Kernel kernel, ILogger logger) 
        : base(kernel, logger, "Checker") { }

    public override async Task<AgentResponse<object>> ExecuteAsync(
        AgentExecutionContext context, 
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            _logger.LogInformation("Checker agent starting execution for {Operation}", context.Operation);
            
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
            _logger.LogError(ex, "Checker agent failed for {Operation}", context.Operation);
            return AgentResponse<object>.Failure($"Checker failed: {ex.Message}");
        }
    }

    public override async IAsyncEnumerable<StreamingChatMessageContent> ExecuteStreamingAsync(
        AgentExecutionContext context,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Checker agent starting streaming execution for {Operation}", context.Operation);
        
        var prompt = BuildPrompt(context);
        
        await foreach (var chunk in ExecuteStreamingChatCompletionAsync(prompt, cancellationToken))
        {
            yield return chunk;
        }
        
        _logger.LogInformation("Checker agent completed streaming execution for {Operation}", context.Operation);
    }

    protected override string BuildPrompt(AgentExecutionContext context)
    {
        return $@"
You are the Checker Agent, responsible for validating operations and ensuring quality.

Current Operation: {context.Operation}
Entity Type: {context.EntityType?.Name ?? "Unknown"}
Context Data: {context.Data?.ToString() ?? "None"}

Your role is to:
1. Validate the operation results
2. Check data integrity and consistency
3. Verify business rule compliance
4. Identify potential issues or anomalies
5. Provide quality assurance feedback

Please perform validation and provide:
- Validation results for each check
- Any issues or warnings found
- Quality metrics and scores
- Recommendations for improvement
- Overall validation status
";
    }

    protected override async Task<object> ProcessResponseAsync(string response, AgentExecutionContext context)
    {
        // Process the checker's response and return structured data
        return new
        {
            ValidationResults = response,
            Operation = context.Operation,
            EntityType = context.EntityType?.Name,
            Timestamp = DateTime.UtcNow,
            Status = "Validated"
        };
    }
} 