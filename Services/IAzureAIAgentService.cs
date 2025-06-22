using UltraGenericSystem.Models;

namespace UltraGenericSystem.Services;

/// <summary>
/// Interface for Azure AI Agent service
/// </summary>
public interface IAzureAIAgentService
{
    /// <summary>
    /// Gets the Azure AI Agent configuration
    /// </summary>
    AzureAIAgentConfig Config { get; }

    /// <summary>
    /// Gets the agent capabilities
    /// </summary>
    AzureAIAgentCapabilities Capabilities { get; }

    /// <summary>
    /// Sends a message to the Azure AI Agent
    /// </summary>
    /// <param name="request">The agent request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The agent response</returns>
    Task<AzureAIAgentResponse> SendMessageAsync(AzureAIAgentRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a streaming message to the Azure AI Agent
    /// </summary>
    /// <param name="request">The agent request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Stream of response chunks</returns>
    IAsyncEnumerable<AzureAIStreamingResponse> SendStreamingMessageAsync(AzureAIAgentRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates embeddings for text
    /// </summary>
    /// <param name="text">Text to embed</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Embedding vector</returns>
    Task<float[]> GenerateEmbeddingsAsync(string text, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates embeddings for multiple texts
    /// </summary>
    /// <param name="texts">Texts to embed</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of embedding vectors</returns>
    Task<List<float[]>> GenerateEmbeddingsAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates cosine similarity between two embeddings
    /// </summary>
    /// <param name="embedding1">First embedding</param>
    /// <param name="embedding2">Second embedding</param>
    /// <returns>Similarity score</returns>
    double CalculateSimilarity(float[] embedding1, float[] embedding2);

    /// <summary>
    /// Finds similar texts using embeddings
    /// </summary>
    /// <param name="queryText">Query text</param>
    /// <param name="candidateTexts">Candidate texts</param>
    /// <param name="threshold">Similarity threshold</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Similar texts with scores</returns>
    Task<List<(string Text, double Score)>> FindSimilarTextsAsync(string queryText, IEnumerable<string> candidateTexts, double threshold = 0.8, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a function call
    /// </summary>
    /// <param name="functionCall">Function call to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Function call result</returns>
    Task<AzureAIFunctionCallResult> ExecuteFunctionCallAsync(AzureAIFunctionCall functionCall, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a function for the agent
    /// </summary>
    /// <param name="functionDefinition">Function definition</param>
    /// <param name="function">Function implementation</param>
    void RegisterFunction(AzureAIFunctionDefinition functionDefinition, Func<string, Task<string>> function);

    /// <summary>
    /// Gets conversation history for a session
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <returns>Conversation history</returns>
    Task<List<AzureAIConversationMessage>> GetConversationHistoryAsync(string sessionId);

    /// <summary>
    /// Saves conversation message
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <param name="message">Message to save</param>
    Task SaveConversationMessageAsync(string sessionId, AzureAIConversationMessage message);

    /// <summary>
    /// Clears conversation history for a session
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    Task ClearConversationHistoryAsync(string sessionId);

    /// <summary>
    /// Analyzes text using the agent
    /// </summary>
    /// <param name="text">Text to analyze</param>
    /// <param name="analysisType">Type of analysis</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Analysis result</returns>
    Task<string> AnalyzeTextAsync(string text, string analysisType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates content using the agent
    /// </summary>
    /// <param name="prompt">Generation prompt</param>
    /// <param name="parameters">Generation parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Generated content</returns>
    Task<string> GenerateContentAsync(string prompt, Dictionary<string, object> parameters, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates the agent configuration
    /// </summary>
    /// <returns>Validation result</returns>
    Task<bool> ValidateConfigurationAsync();

    /// <summary>
    /// Gets agent health status
    /// </summary>
    /// <returns>Health status</returns>
    Task<AzureAIAgentHealthStatus> GetHealthStatusAsync();
}

/// <summary>
/// Azure AI Agent health status
/// </summary>
public class AzureAIAgentHealthStatus
{
    /// <summary>
    /// Whether the agent is healthy
    /// </summary>
    public bool IsHealthy { get; set; }

    /// <summary>
    /// Health status message
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Last health check time
    /// </summary>
    public DateTime LastCheck { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Response time in milliseconds
    /// </summary>
    public long ResponseTimeMs { get; set; }

    /// <summary>
    /// Error details if unhealthy
    /// </summary>
    public string? ErrorDetails { get; set; }

    /// <summary>
    /// Available capabilities
    /// </summary>
    public List<string> AvailableCapabilities { get; set; } = new();
} 