using Microsoft.AspNetCore.Mvc;
using UltraGenericSystem.Models;
using UltraGenericSystem.Services;

namespace UltraGenericSystem.Controllers;

/// <summary>
/// Azure AI Agent controller for managing Azure OpenAI interactions
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AzureAIAgentController : ControllerBase
{
    private readonly IAzureAIAgentService _azureAIAgentService;
    private readonly ILogger<AzureAIAgentController> _logger;

    public AzureAIAgentController(IAzureAIAgentService azureAIAgentService, ILogger<AzureAIAgentController> logger)
    {
        _azureAIAgentService = azureAIAgentService;
        _logger = logger;
    }

    /// <summary>
    /// Send a message to the Azure AI Agent
    /// </summary>
    /// <param name="request">The agent request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The agent response</returns>
    [HttpPost("message")]
    public async Task<ActionResult<AzureAIAgentResponse>> SendMessageAsync(
        [FromBody] AzureAIAgentRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Received message request for session: {SessionId}", request.SessionId);
            
            var response = await _azureAIAgentService.SendMessageAsync(request, cancellationToken);
            
            _logger.LogInformation("Message processed successfully for session: {SessionId}", request.SessionId);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message for session: {SessionId}", request.SessionId);
            return StatusCode(500, new { error = "Failed to process message", details = ex.Message });
        }
    }

    /// <summary>
    /// Send a streaming message to the Azure AI Agent
    /// </summary>
    /// <param name="request">The agent request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Stream of response chunks</returns>
    [HttpPost("stream")]
    public IAsyncEnumerable<AzureAIStreamingResponse> SendStreamingMessageAsync(
        [FromBody] AzureAIAgentRequest request,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Received streaming message request for session: {SessionId}", request.SessionId);
        
        return SendStreamingMessageInternalAsync(request, cancellationToken);
    }

    private async IAsyncEnumerable<AzureAIStreamingResponse> SendStreamingMessageInternalAsync(
        AzureAIAgentRequest request,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var chunk in _azureAIAgentService.SendStreamingMessageAsync(request, cancellationToken))
        {
            yield return chunk;
        }
        
        _logger.LogInformation("Streaming message processed successfully for session: {SessionId}", request.SessionId);
    }

    /// <summary>
    /// Generate embeddings for text
    /// </summary>
    /// <param name="text">Text to embed</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Embedding vector</returns>
    [HttpPost("embeddings")]
    public async Task<ActionResult<float[]>> GenerateEmbeddingsAsync(
        [FromBody] string text,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating embeddings for text");
            
            var embeddings = await _azureAIAgentService.GenerateEmbeddingsAsync(text, cancellationToken);
            
            _logger.LogInformation("Embeddings generated successfully");
            return Ok(embeddings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embeddings");
            return StatusCode(500, new { error = "Failed to generate embeddings", details = ex.Message });
        }
    }

    /// <summary>
    /// Generate embeddings for multiple texts
    /// </summary>
    /// <param name="texts">Texts to embed</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of embedding vectors</returns>
    [HttpPost("embeddings/batch")]
    public async Task<ActionResult<List<float[]>>> GenerateEmbeddingsBatchAsync(
        [FromBody] List<string> texts,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating embeddings for {Count} texts", texts.Count);
            
            var embeddings = await _azureAIAgentService.GenerateEmbeddingsAsync(texts, cancellationToken);
            
            _logger.LogInformation("Batch embeddings generated successfully");
            return Ok(embeddings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating batch embeddings");
            return StatusCode(500, new { error = "Failed to generate batch embeddings", details = ex.Message });
        }
    }

    /// <summary>
    /// Find similar texts using embeddings
    /// </summary>
    /// <param name="request">Similarity search request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Similar texts with scores</returns>
    [HttpPost("similarity")]
    public async Task<ActionResult<List<(string Text, double Score)>>> FindSimilarTextsAsync(
        [FromBody] SimilaritySearchRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Finding similar texts for query: {Query}", request.QueryText);
            
            var results = await _azureAIAgentService.FindSimilarTextsAsync(
                request.QueryText, 
                request.CandidateTexts, 
                request.Threshold, 
                cancellationToken);
            
            _logger.LogInformation("Found {Count} similar texts", results.Count);
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding similar texts");
            return StatusCode(500, new { error = "Failed to find similar texts", details = ex.Message });
        }
    }

    /// <summary>
    /// Execute a function call
    /// </summary>
    /// <param name="functionCall">Function call to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Function call result</returns>
    [HttpPost("function")]
    public async Task<ActionResult<AzureAIFunctionCallResult>> ExecuteFunctionCallAsync(
        [FromBody] AzureAIFunctionCall functionCall,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Executing function call: {FunctionName}", functionCall.Name);
            
            var result = await _azureAIAgentService.ExecuteFunctionCallAsync(functionCall, cancellationToken);
            
            _logger.LogInformation("Function call executed successfully: {FunctionName}", functionCall.Name);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing function call: {FunctionName}", functionCall.Name);
            return StatusCode(500, new { error = "Failed to execute function call", details = ex.Message });
        }
    }

    /// <summary>
    /// Get conversation history for a session
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <returns>Conversation history</returns>
    [HttpGet("conversation/{sessionId}")]
    public async Task<ActionResult<List<AzureAIConversationMessage>>> GetConversationHistoryAsync(string sessionId)
    {
        try
        {
            _logger.LogInformation("Getting conversation history for session: {SessionId}", sessionId);
            
            var history = await _azureAIAgentService.GetConversationHistoryAsync(sessionId);
            
            _logger.LogInformation("Retrieved {Count} messages for session: {SessionId}", history.Count, sessionId);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting conversation history for session: {SessionId}", sessionId);
            return StatusCode(500, new { error = "Failed to get conversation history", details = ex.Message });
        }
    }

    /// <summary>
    /// Clear conversation history for a session
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("conversation/{sessionId}")]
    public async Task<ActionResult> ClearConversationHistoryAsync(string sessionId)
    {
        try
        {
            _logger.LogInformation("Clearing conversation history for session: {SessionId}", sessionId);
            
            await _azureAIAgentService.ClearConversationHistoryAsync(sessionId);
            
            _logger.LogInformation("Conversation history cleared for session: {SessionId}", sessionId);
            return Ok(new { message = "Conversation history cleared successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing conversation history for session: {SessionId}", sessionId);
            return StatusCode(500, new { error = "Failed to clear conversation history", details = ex.Message });
        }
    }

    /// <summary>
    /// Analyze text using the agent
    /// </summary>
    /// <param name="request">Text analysis request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Analysis result</returns>
    [HttpPost("analyze")]
    public async Task<ActionResult<string>> AnalyzeTextAsync(
        [FromBody] TextAnalysisRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Analyzing text for type: {AnalysisType}", request.AnalysisType);
            
            var result = await _azureAIAgentService.AnalyzeTextAsync(request.Text, request.AnalysisType, cancellationToken);
            
            _logger.LogInformation("Text analysis completed for type: {AnalysisType}", request.AnalysisType);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing text for type: {AnalysisType}", request.AnalysisType);
            return StatusCode(500, new { error = "Failed to analyze text", details = ex.Message });
        }
    }

    /// <summary>
    /// Generate content using the agent
    /// </summary>
    /// <param name="request">Content generation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Generated content</returns>
    [HttpPost("generate")]
    public async Task<ActionResult<string>> GenerateContentAsync(
        [FromBody] ContentGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating content with prompt");
            
            var result = await _azureAIAgentService.GenerateContentAsync(request.Prompt, request.Parameters, cancellationToken);
            
            _logger.LogInformation("Content generation completed");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating content");
            return StatusCode(500, new { error = "Failed to generate content", details = ex.Message });
        }
    }

    /// <summary>
    /// Get agent configuration
    /// </summary>
    /// <returns>Agent configuration</returns>
    [HttpGet("config")]
    public ActionResult<AzureAIAgentConfig> GetConfiguration()
    {
        try
        {
            _logger.LogInformation("Getting agent configuration");
            
            var config = _azureAIAgentService.Config;
            
            // Mask sensitive information
            var safeConfig = new AzureAIAgentConfig
            {
                Endpoint = config.Endpoint,
                ApiKey = "***MASKED***",
                ModelDeploymentName = config.ModelDeploymentName,
                EmbeddingDeploymentName = config.EmbeddingDeploymentName,
                MaxTokens = config.MaxTokens,
                Temperature = config.Temperature,
                TopP = config.TopP,
                FrequencyPenalty = config.FrequencyPenalty,
                PresencePenalty = config.PresencePenalty,
                EnableStreaming = config.EnableStreaming,
                EnableFunctionCalling = config.EnableFunctionCalling,
                EnableVision = config.EnableVision,
                CustomHeaders = config.CustomHeaders,
                RetryConfig = config.RetryConfig,
                MemoryConfig = config.MemoryConfig
            };
            
            _logger.LogInformation("Configuration retrieved successfully");
            return Ok(safeConfig);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting configuration");
            return StatusCode(500, new { error = "Failed to get configuration", details = ex.Message });
        }
    }

    /// <summary>
    /// Get agent capabilities
    /// </summary>
    /// <returns>Agent capabilities</returns>
    [HttpGet("capabilities")]
    public ActionResult<AzureAIAgentCapabilities> GetCapabilities()
    {
        try
        {
            _logger.LogInformation("Getting agent capabilities");
            
            var capabilities = _azureAIAgentService.Capabilities;
            
            _logger.LogInformation("Capabilities retrieved successfully");
            return Ok(capabilities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting capabilities");
            return StatusCode(500, new { error = "Failed to get capabilities", details = ex.Message });
        }
    }

    /// <summary>
    /// Validate agent configuration
    /// </summary>
    /// <returns>Validation result</returns>
    [HttpPost("validate")]
    public async Task<ActionResult<bool>> ValidateConfigurationAsync()
    {
        try
        {
            _logger.LogInformation("Validating agent configuration");
            
            var isValid = await _azureAIAgentService.ValidateConfigurationAsync();
            
            _logger.LogInformation("Configuration validation completed: {IsValid}", isValid);
            return Ok(isValid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating configuration");
            return StatusCode(500, new { error = "Failed to validate configuration", details = ex.Message });
        }
    }

    /// <summary>
    /// Get agent health status
    /// </summary>
    /// <returns>Health status</returns>
    [HttpGet("health")]
    public async Task<ActionResult<AzureAIAgentHealthStatus>> GetHealthStatusAsync()
    {
        try
        {
            _logger.LogInformation("Getting agent health status");
            
            var healthStatus = await _azureAIAgentService.GetHealthStatusAsync();
            
            _logger.LogInformation("Health status retrieved: {Status}", healthStatus.Status);
            return Ok(healthStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting health status");
            return StatusCode(500, new { error = "Failed to get health status", details = ex.Message });
        }
    }
}

/// <summary>
/// Similarity search request
/// </summary>
public class SimilaritySearchRequest
{
    /// <summary>
    /// Query text
    /// </summary>
    public string QueryText { get; set; } = string.Empty;

    /// <summary>
    /// Candidate texts to search
    /// </summary>
    public List<string> CandidateTexts { get; set; } = new();

    /// <summary>
    /// Similarity threshold
    /// </summary>
    public double Threshold { get; set; } = 0.8;
}

/// <summary>
/// Text analysis request
/// </summary>
public class TextAnalysisRequest
{
    /// <summary>
    /// Text to analyze
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Type of analysis
    /// </summary>
    public string AnalysisType { get; set; } = string.Empty;
}

/// <summary>
/// Content generation request
/// </summary>
public class ContentGenerationRequest
{
    /// <summary>
    /// Generation prompt
    /// </summary>
    public string Prompt { get; set; } = string.Empty;

    /// <summary>
    /// Generation parameters
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new();
} 