using Microsoft.AspNetCore.Mvc;
using UltraGenericSystem.Models;
using UltraGenericSystem.Services;

namespace UltraGenericSystem.Controllers;

/// <summary>
/// Controller for Ollama LLM operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OllamaController : ControllerBase
{
    private readonly IOllamaService _ollamaService;
    private readonly ILogger<OllamaController> _logger;

    public OllamaController(IOllamaService ollamaService, ILogger<OllamaController> logger)
    {
        _ollamaService = ollamaService;
        _logger = logger;
    }

    /// <summary>
    /// Send a message to Ollama and get a response
    /// </summary>
    [HttpPost("chat")]
    public async Task<ActionResult<OllamaResponse>> SendMessage([FromBody] OllamaRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending message to Ollama model: {Model}", request.Model);
            
            var response = await _ollamaService.SendMessageAsync(request, cancellationToken);
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message to Ollama");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Send a streaming message to Ollama
    /// </summary>
    [HttpPost("chat/stream")]
    public async Task<IActionResult> SendStreamingMessage([FromBody] OllamaRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending streaming message to Ollama model: {Model}", request.Model);
            
            Response.Headers.Add("Content-Type", "text/event-stream");
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Connection", "keep-alive");

            await foreach (var streamingResponse in _ollamaService.SendStreamingMessageAsync(request, cancellationToken))
            {
                var json = System.Text.Json.JsonSerializer.Serialize(streamingResponse);
                await Response.WriteAsync($"data: {json}\n\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
                
                if (streamingResponse.Done)
                {
                    break;
                }
            }

            await Response.WriteAsync("data: [DONE]\n\n", cancellationToken);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in streaming message to Ollama");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Generate embeddings for text
    /// </summary>
    [HttpPost("embeddings")]
    public async Task<ActionResult<object>> GenerateEmbeddings([FromBody] EmbeddingRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating embeddings for text of length: {Length}", request.Text.Length);
            
            var embedding = await _ollamaService.GenerateEmbeddingAsync(request.Text, cancellationToken);
            
            return Ok(new
            {
                embedding = embedding,
                dimensions = embedding.Length,
                model = _ollamaService.Config.DefaultModel,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embeddings");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// List available models on the Ollama server
    /// </summary>
    [HttpGet("models")]
    public async Task<ActionResult<object>> ListModels(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Listing available models from Ollama server");
            
            var models = await _ollamaService.ListModelsAsync(cancellationToken);
            
            return Ok(new
            {
                models = models,
                count = models.Count,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing models");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Check if a model is available
    /// </summary>
    [HttpGet("models/{modelName}/available")]
    public async Task<ActionResult<object>> CheckModelAvailability(string modelName, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Checking availability for model: {ModelName}", modelName);
            
            var isAvailable = await _ollamaService.IsModelAvailableAsync(modelName, cancellationToken);
            
            return Ok(new
            {
                model = modelName,
                available = isAvailable,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking model availability: {ModelName}", modelName);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Pull a model from Ollama registry
    /// </summary>
    [HttpPost("models/pull")]
    public async Task<ActionResult<object>> PullModel([FromBody] PullModelRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Pulling model from Ollama registry: {ModelName}", request.ModelName);
            
            var success = await _ollamaService.PullModelAsync(request.ModelName, cancellationToken);
            
            return Ok(new
            {
                model = request.ModelName,
                success = success,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pulling model: {ModelName}", request.ModelName);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get model information
    /// </summary>
    [HttpGet("models/{modelName}/info")]
    public async Task<ActionResult<object>> GetModelInfo(string modelName, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting model info for: {ModelName}", modelName);
            
            var modelInfo = await _ollamaService.GetModelInfoAsync(modelName, cancellationToken);
            
            if (modelInfo == null)
            {
                return NotFound(new { error = $"Model {modelName} not found" });
            }
            
            return Ok(modelInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting model info: {ModelName}", modelName);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get Ollama service configuration and capabilities
    /// </summary>
    [HttpGet("info")]
    public ActionResult<object> GetServiceInfo()
    {
        return Ok(new
        {
            config = _ollamaService.Config,
            capabilities = _ollamaService.Capabilities,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Test Ollama connection
    /// </summary>
    [HttpGet("health")]
    public async Task<ActionResult<object>> HealthCheck(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Performing Ollama health check");
            
            var models = await _ollamaService.ListModelsAsync(cancellationToken);
            
            return Ok(new
            {
                status = "healthy",
                endpoint = _ollamaService.Config.Endpoint,
                availableModels = models.Count,
                defaultModel = _ollamaService.Config.DefaultModel,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ollama health check failed");
            return StatusCode(503, new
            {
                status = "unhealthy",
                error = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }
}

/// <summary>
/// Request model for embedding generation
/// </summary>
public class EmbeddingRequest
{
    /// <summary>
    /// Text to generate embeddings for
    /// </summary>
    public string Text { get; set; } = string.Empty;
}

/// <summary>
/// Request model for pulling a model
/// </summary>
public class PullModelRequest
{
    /// <summary>
    /// Name of the model to pull
    /// </summary>
    public string ModelName { get; set; } = string.Empty;
} 