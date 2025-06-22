using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;
using UltraGenericSystem.Models;

namespace UltraGenericSystem.Services.Agents;

/// <summary>
/// Reflector agent that analyzes execution results and provides insights
/// </summary>
public class StreamingReflectorAgent : StreamingSKAgentBase
{
    public StreamingReflectorAgent(Kernel kernel, ILogger logger) 
        : base(kernel, logger, "Reflector") { }

    public override async Task<AgentResponse<object>> ExecuteAsync(
        AgentExecutionContext context, 
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            _logger.LogInformation("Reflector agent starting execution for {Operation}", context.Operation);
            
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
            _logger.LogError(ex, "Reflector agent failed for {Operation}", context.Operation);
            return AgentResponse<object>.Failure($"Reflector failed: {ex.Message}");
        }
    }

    public override async IAsyncEnumerable<StreamingChatMessageContent> ExecuteStreamingAsync(
        AgentExecutionContext context,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Reflector agent starting streaming execution for {Operation}", context.Operation);
        
        var prompt = BuildPrompt(context);
        
        await foreach (var chunk in ExecuteStreamingChatCompletionAsync(prompt, cancellationToken))
        {
            yield return chunk;
        }
        
        _logger.LogInformation("Reflector agent completed streaming execution for {Operation}", context.Operation);
    }

    protected override string BuildPrompt(AgentExecutionContext context)
    {
        return $@"
You are the Reflector Agent, responsible for analyzing execution results and providing insights.

Current Operation: {context.Operation}
Entity Type: {context.EntityType?.Name ?? "Unknown"}
Context Data: {context.Data?.ToString() ?? "None"}

Your role is to:
1. Analyze the execution results and performance
2. Identify patterns and trends
3. Provide insights and recommendations
4. Suggest optimizations and improvements
5. Document lessons learned

Please analyze the operation and provide:
- Performance analysis and metrics
- Pattern recognition and insights
- Optimization opportunities
- Lessons learned and best practices
- Recommendations for future improvements
";
    }

    protected override async Task<object> ProcessResponseAsync(string response, AgentExecutionContext context)
    {
        // Process the reflector's response and return structured data
        return new
        {
            AnalysisResults = response,
            Operation = context.Operation,
            EntityType = context.EntityType?.Name,
            Timestamp = DateTime.UtcNow,
            Status = "Analyzed"
        };
    }
} 