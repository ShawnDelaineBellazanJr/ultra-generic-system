using UltraGenericSystem.Models;

namespace UltraGenericSystem.Services;

/// <summary>
/// Interface for Ollama LLM service integration
/// </summary>
public interface IOllamaService
{
    /// <summary>
    /// Gets the Ollama configuration
    /// </summary>
    OllamaConfig Config { get; }

    /// <summary>
    /// Gets the Ollama capabilities
    /// </summary>
    OllamaCapabilities Capabilities { get; }

    /// <summary>
    /// Sends a message to Ollama and gets a response
    /// </summary>
    Task<OllamaResponse> SendMessageAsync(OllamaRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a streaming message to Ollama
    /// </summary>
    IAsyncEnumerable<OllamaStreamingResponse> SendStreamingMessageAsync(OllamaRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates embeddings using Ollama
    /// </summary>
    Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists available models on the Ollama server
    /// </summary>
    Task<List<string>> ListModelsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a model is available on the Ollama server
    /// </summary>
    Task<bool> IsModelAvailableAsync(string modelName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Pulls a model from Ollama registry
    /// </summary>
    Task<bool> PullModelAsync(string modelName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets model information
    /// </summary>
    Task<OllamaModelInfo?> GetModelInfoAsync(string modelName, CancellationToken cancellationToken = default);
} 