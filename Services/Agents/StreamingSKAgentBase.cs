using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using UltraGenericSystem.Models;

namespace UltraGenericSystem.Services.Agents;

/// <summary>
/// Base class for streaming SK agents with real-time response capabilities
/// </summary>
public abstract class StreamingSKAgentBase
{
    protected readonly Kernel _kernel;
    protected readonly ILogger _logger;
    protected readonly string _agentName;
    
    public StreamingSKAgentBase(Kernel kernel, ILogger logger, string agentName)
    {
        _kernel = kernel;
        _logger = logger;
        _agentName = agentName;
    }
    
    /// <summary>
    /// Execute the agent operation and return a complete response
    /// </summary>
    public abstract Task<AgentResponse<object>> ExecuteAsync(
        AgentExecutionContext context, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Execute the agent operation with streaming response
    /// </summary>
    public abstract IAsyncEnumerable<StreamingChatMessageContent> ExecuteStreamingAsync(
        AgentExecutionContext context,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Build a context-aware prompt for the agent
    /// </summary>
    protected abstract string BuildPrompt(AgentExecutionContext context);
    
    /// <summary>
    /// Process the agent's response and convert to appropriate format
    /// </summary>
    protected abstract Task<object> ProcessResponseAsync(string response, AgentExecutionContext context);
    
    /// <summary>
    /// Create a streaming chat message content
    /// </summary>
    protected StreamingChatMessageContent CreateStreamingContent(string content)
    {
        return new StreamingChatMessageContent(AuthorRole.Assistant, content);
    }
    
    /// <summary>
    /// Execute chat completion with the given prompt using SK 1.57.0 API
    /// </summary>
    protected async Task<string> ExecuteChatCompletionAsync(string prompt, CancellationToken cancellationToken = default)
    {
        try
        {
            // Use the kernel directly with a simple prompt
            var result = await _kernel.InvokePromptAsync(prompt, cancellationToken: cancellationToken);
            return result.GetValue<string>() ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing chat completion for {AgentName}", _agentName);
            return $"Error: {ex.Message}";
        }
    }
    
    /// <summary>
    /// Execute streaming chat completion with the given prompt using SK 1.57.0 API
    /// </summary>
    protected async IAsyncEnumerable<StreamingChatMessageContent> ExecuteStreamingChatCompletionAsync(
        string prompt,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Use the kernel directly for streaming
        await foreach (var chunk in _kernel.InvokePromptStreamingAsync(prompt, cancellationToken: cancellationToken))
        {
            var content = chunk.ToString();
            if (!string.IsNullOrEmpty(content))
            {
                yield return CreateStreamingContent(content);
            }
        }
    }
    
    /// <summary>
    /// Log agent execution details
    /// </summary>
    protected void LogExecution(string operation, TimeSpan duration, bool success = true)
    {
        if (success)
        {
            _logger.LogInformation("Agent {AgentName} completed {Operation} in {Duration}ms", 
                _agentName, operation, duration.TotalMilliseconds);
        }
        else
        {
            _logger.LogWarning("Agent {AgentName} failed {Operation} after {Duration}ms", 
                _agentName, operation, duration.TotalMilliseconds);
        }
    }
} 