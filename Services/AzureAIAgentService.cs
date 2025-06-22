using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.TextGeneration;
using System.Text.Json;
using UltraGenericSystem.Models;

namespace UltraGenericSystem.Services;

/// <summary>
/// Azure AI Agent service implementation
/// </summary>
public class AzureAIAgentService : IAzureAIAgentService
{
    private readonly ILogger<AzureAIAgentService> _logger;
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatService;
    private readonly Dictionary<string, Func<string, Task<string>>> _registeredFunctions;
    private readonly Dictionary<string, List<AzureAIConversationMessage>> _conversationHistory;

    public AzureAIAgentConfig Config { get; }
    public AzureAIAgentCapabilities Capabilities { get; }

    public AzureAIAgentService(ILogger<AzureAIAgentService> logger, AzureAIAgentConfig config)
    {
        _logger = logger;
        Config = config;
        _registeredFunctions = new Dictionary<string, Func<string, Task<string>>>();
        _conversationHistory = new Dictionary<string, List<AzureAIConversationMessage>>();

        // Initialize capabilities
        Capabilities = new AzureAIAgentCapabilities
        {
            SupportsTextGeneration = true,
            SupportsFunctionCalling = true,
            SupportsVision = config.EnableVision,
            SupportsStreaming = config.EnableStreaming,
            SupportsMemory = config.MemoryConfig.EnableMemory,
            SupportsEmbeddings = true,
            MaxInputTokens = 128000,
            MaxOutputTokens = config.MaxTokens,
            SupportedModels = new List<string> { "gpt-4", "gpt-35-turbo", "gpt-4o" }
        };

        // Initialize kernel
        var builder = Kernel.CreateBuilder();
        
        // Add Azure OpenAI chat completion
        builder.AddAzureOpenAIChatCompletion(
            deploymentName: config.ModelDeploymentName,
            endpoint: config.Endpoint,
            apiKey: config.ApiKey,
            modelId: config.ModelDeploymentName);

        _kernel = builder.Build();
        _chatService = _kernel.GetRequiredService<IChatCompletionService>();

        _logger.LogInformation("Azure AI Agent Service initialized with model: {Model}", config.ModelDeploymentName);
    }

    public async Task<AzureAIAgentResponse> SendMessageAsync(AzureAIAgentRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing message for session: {SessionId}", request.SessionId);

            // Build the prompt
            var prompt = BuildPrompt(request);

            // Get response using kernel
            var result = await _kernel.InvokePromptAsync(prompt, cancellationToken: cancellationToken);
            var response = result.GetValue<string>() ?? string.Empty;

            // Save conversation message
            var userMessage = new AzureAIConversationMessage
            {
                Role = "user",
                Content = request.Message,
                MessageId = Guid.NewGuid().ToString()
            };
            await SaveConversationMessageAsync(request.SessionId, userMessage);

            var assistantMessage = new AzureAIConversationMessage
            {
                Role = "assistant",
                Content = response,
                MessageId = Guid.NewGuid().ToString()
            };
            await SaveConversationMessageAsync(request.SessionId, assistantMessage);

            // Build response
            var agentResponse = new AzureAIAgentResponse
            {
                Message = response,
                ToolCalls = new List<AzureAIToolCall>(),
                ToolCallResults = new List<AzureAIToolCallResult>(),
                Usage = new AzureAIUsage
                {
                    PromptTokens = 0, // Not available with kernel.InvokePromptAsync
                    CompletionTokens = 0,
                    TotalTokens = 0,
                    Model = Config.ModelDeploymentName
                },
                IsComplete = true,
                ResponseId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow
            };

            _logger.LogInformation("Message processed successfully for session: {SessionId}", request.SessionId);
            return agentResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message for session: {SessionId}", request.SessionId);
            throw;
        }
    }

    public async IAsyncEnumerable<AzureAIStreamingResponse> SendStreamingMessageAsync(AzureAIAgentRequest request, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing streaming message for session: {SessionId}", request.SessionId);

        // Build the prompt
        var prompt = BuildPrompt(request);

        // Get streaming response using kernel
        var fullContent = new System.Text.StringBuilder();

        await foreach (var chunk in _kernel.InvokePromptStreamingAsync(prompt, cancellationToken: cancellationToken))
        {
            var content = chunk.ToString();
            fullContent.Append(content);

            var streamingChunk = new AzureAIStreamingResponse
            {
                Content = content,
                IsFinal = false, // We don't know if it's final with kernel streaming
                ChunkId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow,
                ToolCalls = new List<AzureAIToolCall>()
            };

            yield return streamingChunk;
        }

        // Send final chunk
        yield return new AzureAIStreamingResponse
        {
            Content = string.Empty,
            IsFinal = true,
            ChunkId = Guid.NewGuid().ToString(),
            Timestamp = DateTime.UtcNow,
            ToolCalls = new List<AzureAIToolCall>()
        };

        // Save conversation message
        var userMessage = new AzureAIConversationMessage
        {
            Role = "user",
            Content = request.Message,
            MessageId = Guid.NewGuid().ToString()
        };
        await SaveConversationMessageAsync(request.SessionId, userMessage);

        var assistantMessage = new AzureAIConversationMessage
        {
            Role = "assistant",
            Content = fullContent.ToString(),
            MessageId = Guid.NewGuid().ToString(),
            ToolCalls = new List<AzureAIToolCall>()
        };
        await SaveConversationMessageAsync(request.SessionId, assistantMessage);

        _logger.LogInformation("Streaming message processed successfully for session: {SessionId}", request.SessionId);
    }

    private string BuildPrompt(AzureAIAgentRequest request)
    {
        var prompt = new System.Text.StringBuilder();

        // Add system prompt if provided
        if (!string.IsNullOrEmpty(request.SystemPrompt))
        {
            prompt.AppendLine(request.SystemPrompt);
            prompt.AppendLine();
        }

        // Add conversation history
        foreach (var message in request.ConversationHistory)
        {
            switch (message.Role.ToLower())
            {
                case "system":
                    prompt.AppendLine($"System: {message.Content}");
                    break;
                case "user":
                    prompt.AppendLine($"User: {message.Content}");
                    break;
                case "assistant":
                    prompt.AppendLine($"Assistant: {message.Content}");
                    break;
            }
            prompt.AppendLine();
        }

        // Add current user message
        prompt.AppendLine($"User: {request.Message}");
        prompt.AppendLine("Assistant:");

        return prompt.ToString();
    }

    public async Task<float[]> GenerateEmbeddingsAsync(string text, CancellationToken cancellationToken = default)
    {
        try
        {
            // For now, use a simple hash-based embedding similar to MemoryService
            // In a real implementation, you would use the actual embedding model
            var hash = text.GetHashCode();
            var random = new Random(hash);
            var embedding = new List<float>();

            for (int i = 0; i < 1536; i++) // OpenAI embedding size
            {
                embedding.Add((float)(random.NextDouble() * 2 - 1));
            }

            // Normalize the embedding
            var magnitude = Math.Sqrt(embedding.Sum(x => x * x));
            for (int i = 0; i < embedding.Count; i++)
            {
                embedding[i] = (float)(embedding[i] / magnitude);
            }

            return embedding.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embeddings for text");
            throw;
        }
    }

    public async Task<List<float[]>> GenerateEmbeddingsAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default)
    {
        try
        {
            var embeddings = new List<float[]>();
            foreach (var text in texts)
            {
                var embedding = await GenerateEmbeddingsAsync(text, cancellationToken);
                embeddings.Add(embedding);
            }
            return embeddings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embeddings for multiple texts");
            throw;
        }
    }

    public double CalculateSimilarity(float[] embedding1, float[] embedding2)
    {
        if (embedding1.Length != embedding2.Length)
        {
            throw new ArgumentException("Embeddings must have the same length");
        }

        var dotProduct = 0.0;
        var norm1 = 0.0;
        var norm2 = 0.0;

        for (int i = 0; i < embedding1.Length; i++)
        {
            dotProduct += embedding1[i] * embedding2[i];
            norm1 += embedding1[i] * embedding1[i];
            norm2 += embedding2[i] * embedding2[i];
        }

        norm1 = Math.Sqrt(norm1);
        norm2 = Math.Sqrt(norm2);

        if (norm1 == 0 || norm2 == 0)
        {
            return 0;
        }

        return dotProduct / (norm1 * norm2);
    }

    public async Task<List<(string Text, double Score)>> FindSimilarTextsAsync(string queryText, IEnumerable<string> candidateTexts, double threshold = 0.8, CancellationToken cancellationToken = default)
    {
        try
        {
            var queryEmbedding = await GenerateEmbeddingsAsync(queryText, cancellationToken);
            var candidateEmbeddings = await GenerateEmbeddingsAsync(candidateTexts, cancellationToken);

            var results = new List<(string Text, double Score)>();
            var candidateArray = candidateTexts.ToArray();

            for (int i = 0; i < candidateArray.Length; i++)
            {
                var similarity = CalculateSimilarity(queryEmbedding, candidateEmbeddings[i]);
                if (similarity >= threshold)
                {
                    results.Add((candidateArray[i], similarity));
                }
            }

            return results.OrderByDescending(r => r.Score).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding similar texts");
            throw;
        }
    }

    public async Task<AzureAIFunctionCallResult> ExecuteFunctionCallAsync(AzureAIFunctionCall functionCall, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_registeredFunctions.ContainsKey(functionCall.Name))
            {
                var result = await _registeredFunctions[functionCall.Name](functionCall.Arguments);
                return new AzureAIFunctionCallResult
                {
                    Name = functionCall.Name,
                    Content = result
                };
            }
            else
            {
                throw new InvalidOperationException($"Function '{functionCall.Name}' is not registered");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing function call: {FunctionName}", functionCall.Name);
            throw;
        }
    }

    public void RegisterFunction(AzureAIFunctionDefinition functionDefinition, Func<string, Task<string>> function)
    {
        _registeredFunctions[functionDefinition.Name] = function;
        _logger.LogInformation("Registered function: {FunctionName}", functionDefinition.Name);
    }

    public async Task<List<AzureAIConversationMessage>> GetConversationHistoryAsync(string sessionId)
    {
        if (_conversationHistory.ContainsKey(sessionId))
        {
            return _conversationHistory[sessionId];
        }
        return new List<AzureAIConversationMessage>();
    }

    public async Task SaveConversationMessageAsync(string sessionId, AzureAIConversationMessage message)
    {
        if (!_conversationHistory.ContainsKey(sessionId))
        {
            _conversationHistory[sessionId] = new List<AzureAIConversationMessage>();
        }

        _conversationHistory[sessionId].Add(message);

        // Clean up old messages if memory limit exceeded
        if (_conversationHistory[sessionId].Count > Config.MemoryConfig.MaxMemoryEntries)
        {
            _conversationHistory[sessionId] = _conversationHistory[sessionId]
                .Skip(_conversationHistory[sessionId].Count - Config.MemoryConfig.MaxMemoryEntries)
                .ToList();
        }
    }

    public async Task ClearConversationHistoryAsync(string sessionId)
    {
        if (_conversationHistory.ContainsKey(sessionId))
        {
            _conversationHistory[sessionId].Clear();
        }
    }

    public async Task<string> AnalyzeTextAsync(string text, string analysisType, CancellationToken cancellationToken = default)
    {
        try
        {
            var prompt = $"Please analyze the following text for {analysisType}:\n\n{text}\n\nProvide a detailed analysis.";
            
            var request = new AzureAIAgentRequest
            {
                Message = prompt,
                SystemPrompt = "You are an expert text analyst. Provide detailed, accurate analysis.",
                EnableStreaming = false
            };

            var response = await SendMessageAsync(request, cancellationToken);
            return response.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing text for type: {AnalysisType}", analysisType);
            throw;
        }
    }

    public async Task<string> GenerateContentAsync(string prompt, Dictionary<string, object> parameters, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new AzureAIAgentRequest
            {
                Message = prompt,
                Parameters = parameters,
                EnableStreaming = false
            };

            var response = await SendMessageAsync(request, cancellationToken);
            return response.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating content");
            throw;
        }
    }

    public async Task<bool> ValidateConfigurationAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(Config.Endpoint) || string.IsNullOrEmpty(Config.ApiKey))
            {
                return false;
            }

            // Test with a simple message
            var request = new AzureAIAgentRequest
            {
                Message = "Hello",
                EnableStreaming = false
            };

            await SendMessageAsync(request);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Configuration validation failed");
            return false;
        }
    }

    public async Task<AzureAIAgentHealthStatus> GetHealthStatusAsync()
    {
        var startTime = DateTime.UtcNow;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var isValid = await ValidateConfigurationAsync();
            stopwatch.Stop();

            return new AzureAIAgentHealthStatus
            {
                IsHealthy = isValid,
                Status = isValid ? "Healthy" : "Unhealthy",
                LastCheck = DateTime.UtcNow,
                ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                AvailableCapabilities = new List<string>
                {
                    "Text Generation",
                    "Function Calling",
                    "Embeddings",
                    "Conversation Memory"
                }
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new AzureAIAgentHealthStatus
            {
                IsHealthy = false,
                Status = "Error",
                LastCheck = DateTime.UtcNow,
                ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                ErrorDetails = ex.Message,
                AvailableCapabilities = new List<string>()
            };
        }
    }
} 