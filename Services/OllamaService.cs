using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.TextGeneration;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using UltraGenericSystem.Models;

namespace UltraGenericSystem.Services;

/// <summary>
/// Ollama LLM service implementation with Semantic Kernel integration
/// </summary>
public class OllamaService : IOllamaService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OllamaService> _logger;
    private readonly OllamaConfig _config;
    private readonly OllamaCapabilities _capabilities;

    public OllamaConfig Config => _config;
    public OllamaCapabilities Capabilities => _capabilities;

    public OllamaService(ILogger<OllamaService> logger, OllamaConfig config)
    {
        _logger = logger;
        _config = config;
        _capabilities = new OllamaCapabilities();

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(config.Endpoint),
            Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds)
        };

        _httpClient.DefaultRequestHeaders.Add("User-Agent", "UltraGenericSystem/1.0");

        _logger.LogInformation("Ollama service initialized with endpoint: {Endpoint}", config.Endpoint);
    }

    /// <summary>
    /// Sends a message to Ollama and gets a response
    /// </summary>
    public async Task<OllamaResponse> SendMessageAsync(OllamaRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending message to Ollama model: {Model}", request.Model);

            // Set default options if not provided
            request.Options ??= new OllamaGenerationOptions
            {
                Temperature = _config.Temperature,
                TopP = _config.TopP,
                TopK = _config.TopK,
                NumPredict = _config.MaxTokens,
                RepeatPenalty = _config.RepeatPenalty
            };

            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/api/generate", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var ollamaResponse = JsonSerializer.Deserialize<OllamaResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            if (ollamaResponse == null)
            {
                throw new InvalidOperationException("Failed to deserialize Ollama response");
            }

            _logger.LogInformation("Received response from Ollama model: {Model}", request.Model);
            return ollamaResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message to Ollama model: {Model}", request.Model);
            throw;
        }
    }

    /// <summary>
    /// Sends a streaming message to Ollama
    /// </summary>
    public async IAsyncEnumerable<OllamaStreamingResponse> SendStreamingMessageAsync(
        OllamaRequest request, 
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Enable streaming
        request.Stream = true;

        // Set default options if not provided
        request.Options ??= new OllamaGenerationOptions
        {
            Temperature = _config.Temperature,
            TopP = _config.TopP,
            TopK = _config.TopK,
            NumPredict = _config.MaxTokens,
            RepeatPenalty = _config.RepeatPenalty
        };

        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        });

        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        using var response = await _httpClient.PostAsync("/api/generate", content, cancellationToken);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        string? line;
        while ((line = await reader.ReadLineAsync()) != null && !cancellationToken.IsCancellationRequested)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            OllamaStreamingResponse? streamingResponse = null;
            try
            {
                streamingResponse = JsonSerializer.Deserialize<OllamaStreamingResponse>(line, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse streaming response line: {Line}", line);
                continue;
            }

            if (streamingResponse != null)
            {
                yield return streamingResponse;
            }

            if (streamingResponse?.Done == true)
            {
                break;
            }
        }

        _logger.LogInformation("Completed streaming response from Ollama model: {Model}", request.Model);
    }

    /// <summary>
    /// Generates embeddings using Ollama
    /// </summary>
    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating embedding for text of length: {Length}", text.Length);

            var request = new
            {
                model = _config.DefaultModel,
                prompt = text
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/api/embeddings", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var embeddingResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

            if (embeddingResponse.TryGetProperty("embedding", out var embeddingArray))
            {
                var embedding = embeddingArray.EnumerateArray()
                    .Select(x => x.GetSingle())
                    .ToArray();

                _logger.LogInformation("Generated embedding with {Count} dimensions", embedding.Length);
                return embedding;
            }

            throw new InvalidOperationException("Failed to extract embedding from response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embedding");
            throw;
        }
    }

    /// <summary>
    /// Lists available models on the Ollama server
    /// </summary>
    public async Task<List<string>> ListModelsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Listing available models from Ollama server");

            var response = await _httpClient.GetAsync("/api/tags", cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var modelsResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

            var models = new List<string>();
            if (modelsResponse.TryGetProperty("models", out var modelsArray))
            {
                foreach (var model in modelsArray.EnumerateArray())
                {
                    if (model.TryGetProperty("name", out var name))
                    {
                        models.Add(name.GetString() ?? string.Empty);
                    }
                }
            }

            _logger.LogInformation("Found {Count} available models", models.Count);
            return models;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing models");
            throw;
        }
    }

    /// <summary>
    /// Checks if a model is available on the Ollama server
    /// </summary>
    public async Task<bool> IsModelAvailableAsync(string modelName, CancellationToken cancellationToken = default)
    {
        try
        {
            var availableModels = await ListModelsAsync(cancellationToken);
            return availableModels.Contains(modelName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking model availability: {ModelName}", modelName);
            return false;
        }
    }

    /// <summary>
    /// Pulls a model from Ollama registry
    /// </summary>
    public async Task<bool> PullModelAsync(string modelName, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Pulling model from Ollama registry: {ModelName}", modelName);

            var request = new { name = modelName };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/api/pull", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Successfully pulled model: {ModelName}", modelName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pulling model: {ModelName}", modelName);
            return false;
        }
    }

    /// <summary>
    /// Gets model information
    /// </summary>
    public async Task<OllamaModelInfo?> GetModelInfoAsync(string modelName, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting model info for: {ModelName}", modelName);

            var request = new { name = modelName };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/api/show", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var modelInfo = JsonSerializer.Deserialize<OllamaModelInfo>(responseContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            _logger.LogInformation("Retrieved model info for: {ModelName}", modelName);
            return modelInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting model info: {ModelName}", modelName);
            return null;
        }
    }

    /// <summary>
    /// Creates a Semantic Kernel chat completion service for Ollama
    /// </summary>
    public IChatCompletionService CreateChatCompletionService()
    {
        return new OllamaChatCompletionService(this, _logger);
    }

    /// <summary>
    /// Creates a Semantic Kernel text generation service for Ollama
    /// </summary>
    public ITextGenerationService CreateTextGenerationService()
    {
        return new OllamaTextGenerationService(this, _logger);
    }

    /// <summary>
    /// Disposes the service
    /// </summary>
    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

/// <summary>
/// Semantic Kernel chat completion service for Ollama
/// </summary>
public class OllamaChatCompletionService : IChatCompletionService
{
    private readonly OllamaService _ollamaService;
    private readonly ILogger _logger;

    public OllamaChatCompletionService(OllamaService ollamaService, ILogger logger)
    {
        _ollamaService = ollamaService;
        _logger = logger;
    }

    public IReadOnlyDictionary<string, object?> Attributes => new Dictionary<string, object?>();

    public async Task<ChatMessageContent> GetChatMessageContentAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var lastMessage = chatHistory.LastOrDefault();
            if (lastMessage == null)
            {
                throw new InvalidOperationException("No messages in chat history");
            }

            var systemMessage = chatHistory.FirstOrDefault(m => m.Role == AuthorRole.System);
            var userMessage = lastMessage.Content;

            var request = new OllamaRequest
            {
                Model = _ollamaService.Config.DefaultModel,
                Prompt = userMessage,
                System = systemMessage?.Content,
                Stream = false
            };

            var response = await _ollamaService.SendMessageAsync(request, cancellationToken);
            
            return new ChatMessageContent(AuthorRole.Assistant, response.Response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Ollama chat completion");
            throw;
        }
    }

    public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        var message = await GetChatMessageContentAsync(chatHistory, executionSettings, kernel, cancellationToken);
        return new List<ChatMessageContent> { message };
    }

    public async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var lastMessage = chatHistory.LastOrDefault();
        if (lastMessage == null)
        {
            throw new InvalidOperationException("No messages in chat history");
        }

        var systemMessage = chatHistory.FirstOrDefault(m => m.Role == AuthorRole.System);
        var userMessage = lastMessage.Content;

        var request = new OllamaRequest
        {
            Model = _ollamaService.Config.DefaultModel,
            Prompt = userMessage,
            System = systemMessage?.Content,
            Stream = true
        };

        await foreach (var streamingResponse in _ollamaService.SendStreamingMessageAsync(request, cancellationToken))
        {
            yield return new StreamingChatMessageContent(AuthorRole.Assistant, streamingResponse.Response);
            
            if (streamingResponse.Done)
            {
                break;
            }
        }
    }
}

/// <summary>
/// Semantic Kernel text generation service for Ollama
/// </summary>
public class OllamaTextGenerationService : ITextGenerationService
{
    private readonly OllamaService _ollamaService;
    private readonly ILogger _logger;

    public OllamaTextGenerationService(OllamaService ollamaService, ILogger logger)
    {
        _ollamaService = ollamaService;
        _logger = logger;
    }

    public IReadOnlyDictionary<string, object?> Attributes => new Dictionary<string, object?>();

    public async Task<TextContent> GetTextContentAsync(
        string prompt,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new OllamaRequest
            {
                Model = _ollamaService.Config.DefaultModel,
                Prompt = prompt,
                Stream = false
            };

            var response = await _ollamaService.SendMessageAsync(request, cancellationToken);
            
            return new TextContent(response.Response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Ollama text generation");
            throw;
        }
    }

    public async Task<IReadOnlyList<TextContent>> GetTextContentsAsync(
        string prompt,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        var content = await GetTextContentAsync(prompt, executionSettings, kernel, cancellationToken);
        return new List<TextContent> { content };
    }

    public async IAsyncEnumerable<StreamingTextContent> GetStreamingTextContentsAsync(
        string prompt,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var request = new OllamaRequest
        {
            Model = _ollamaService.Config.DefaultModel,
            Prompt = prompt,
            Stream = true
        };

        await foreach (var streamingResponse in _ollamaService.SendStreamingMessageAsync(request, cancellationToken))
        {
            yield return new StreamingTextContent(streamingResponse.Response);
            
            if (streamingResponse.Done)
            {
                break;
            }
        }
    }
} 