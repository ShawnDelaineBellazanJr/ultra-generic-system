using System.Text.Json.Serialization;

namespace UltraGenericSystem.Models;

/// <summary>
/// Configuration for Ollama service
/// </summary>
public class OllamaConfig
{
    /// <summary>
    /// Ollama server endpoint (default: http://localhost:11434)
    /// </summary>
    public string Endpoint { get; set; } = "http://localhost:11434";

    /// <summary>
    /// Default model to use
    /// </summary>
    public string DefaultModel { get; set; } = "llama3.2";

    /// <summary>
    /// Maximum tokens for generation
    /// </summary>
    public int MaxTokens { get; set; } = 4096;

    /// <summary>
    /// Temperature for generation (0.0 to 1.0)
    /// </summary>
    public double Temperature { get; set; } = 0.7;

    /// <summary>
    /// Top-p sampling parameter
    /// </summary>
    public double TopP { get; set; } = 0.9;

    /// <summary>
    /// Top-k sampling parameter
    /// </summary>
    public int TopK { get; set; } = 40;

    /// <summary>
    /// Repeat penalty
    /// </summary>
    public double RepeatPenalty { get; set; } = 1.1;

    /// <summary>
    /// Enable streaming responses
    /// </summary>
    public bool EnableStreaming { get; set; } = true;

    /// <summary>
    /// Enable function calling
    /// </summary>
    public bool EnableFunctionCalling { get; set; } = true;

    /// <summary>
    /// Timeout for requests in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 120;

    /// <summary>
    /// Retry configuration
    /// </summary>
    public OllamaRetryConfig RetryConfig { get; set; } = new();
}

/// <summary>
/// Retry configuration for Ollama requests
/// </summary>
public class OllamaRetryConfig
{
    /// <summary>
    /// Maximum number of retries
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Delay between retries in seconds
    /// </summary>
    public int RetryDelaySeconds { get; set; } = 2;

    /// <summary>
    /// Exponential backoff multiplier
    /// </summary>
    public double BackoffMultiplier { get; set; } = 2.0;
}

/// <summary>
/// Ollama request model
/// </summary>
public class OllamaRequest
{
    /// <summary>
    /// Model to use for generation
    /// </summary>
    [JsonPropertyName("model")]
    public string Model { get; set; } = "llama3.2";

    /// <summary>
    /// Prompt to send to the model
    /// </summary>
    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = string.Empty;

    /// <summary>
    /// System message for chat models
    /// </summary>
    [JsonPropertyName("system")]
    public string? System { get; set; }

    /// <summary>
    /// Template for prompt formatting
    /// </summary>
    [JsonPropertyName("template")]
    public string? Template { get; set; }

    /// <summary>
    /// Context for conversation history
    /// </summary>
    [JsonPropertyName("context")]
    public long[]? Context { get; set; }

    /// <summary>
    /// Options for generation
    /// </summary>
    [JsonPropertyName("options")]
    public OllamaGenerationOptions? Options { get; set; }

    /// <summary>
    /// Whether to stream the response
    /// </summary>
    [JsonPropertyName("stream")]
    public bool Stream { get; set; } = false;

    /// <summary>
    /// Raw prompt (bypasses template)
    /// </summary>
    [JsonPropertyName("raw")]
    public bool Raw { get; set; } = false;

    /// <summary>
    /// Format for the response
    /// </summary>
    [JsonPropertyName("format")]
    public string? Format { get; set; }

    /// <summary>
    /// Keep alive duration
    /// </summary>
    [JsonPropertyName("keep_alive")]
    public string? KeepAlive { get; set; }
}

/// <summary>
/// Generation options for Ollama
/// </summary>
public class OllamaGenerationOptions
{
    /// <summary>
    /// Temperature for generation
    /// </summary>
    [JsonPropertyName("temperature")]
    public double? Temperature { get; set; }

    /// <summary>
    /// Top-p sampling
    /// </summary>
    [JsonPropertyName("top_p")]
    public double? TopP { get; set; }

    /// <summary>
    /// Top-k sampling
    /// </summary>
    [JsonPropertyName("top_k")]
    public int? TopK { get; set; }

    /// <summary>
    /// Number of tokens to generate
    /// </summary>
    [JsonPropertyName("num_predict")]
    public int? NumPredict { get; set; }

    /// <summary>
    /// Repeat penalty
    /// </summary>
    [JsonPropertyName("repeat_penalty")]
    public double? RepeatPenalty { get; set; }

    /// <summary>
    /// Seed for reproducible generation
    /// </summary>
    [JsonPropertyName("seed")]
    public int? Seed { get; set; }

    /// <summary>
    /// Stop sequences
    /// </summary>
    [JsonPropertyName("stop")]
    public string[]? Stop { get; set; }

    /// <summary>
    /// TFS-Z parameter
    /// </summary>
    [JsonPropertyName("tfs_z")]
    public double? TfsZ { get; set; }

    /// <summary>
    /// Typical-p parameter
    /// </summary>
    [JsonPropertyName("typical_p")]
    public double? TypicalP { get; set; }

    /// <summary>
    /// Mirostat mode
    /// </summary>
    [JsonPropertyName("mirostat")]
    public int? Mirostat { get; set; }

    /// <summary>
    /// Mirostat tau
    /// </summary>
    [JsonPropertyName("mirostat_tau")]
    public double? MirostatTau { get; set; }

    /// <summary>
    /// Mirostat eta
    /// </summary>
    [JsonPropertyName("mirostat_eta")]
    public double? MirostatEta { get; set; }

    /// <summary>
    /// Grammar for constrained generation
    /// </summary>
    [JsonPropertyName("grammar")]
    public string? Grammar { get; set; }

    /// <summary>
    /// Number of parallel requests
    /// </summary>
    [JsonPropertyName("num_ctx")]
    public int? NumCtx { get; set; }

    /// <summary>
    /// Number of threads
    /// </summary>
    [JsonPropertyName("num_thread")]
    public int? NumThread { get; set; }

    /// <summary>
    /// Number of GPU layers
    /// </summary>
    [JsonPropertyName("num_gpu")]
    public int? NumGpu { get; set; }

    /// <summary>
    /// Main GPU
    /// </summary>
    [JsonPropertyName("main_gpu")]
    public int? MainGpu { get; set; }

    /// <summary>
    /// Low VRAM mode
    /// </summary>
    [JsonPropertyName("low_vram")]
    public bool? LowVram { get; set; }

    /// <summary>
    /// F16 KV
    /// </summary>
    [JsonPropertyName("f16_kv")]
    public bool? F16Kv { get; set; }

    /// <summary>
    /// Logits all
    /// </summary>
    [JsonPropertyName("logits_all")]
    public bool? LogitsAll { get; set; }

    /// <summary>
    /// Vocab only
    /// </summary>
    [JsonPropertyName("vocab_only")]
    public bool? VocabOnly { get; set; }

    /// <summary>
    /// Use MLock
    /// </summary>
    [JsonPropertyName("use_mlock")]
    public bool? UseMlock { get; set; }

    /// <summary>
    /// Use MMAP
    /// </summary>
    [JsonPropertyName("use_mmap")]
    public bool? UseMmap { get; set; }

    /// <summary>
    /// Rope frequency base
    /// </summary>
    [JsonPropertyName("rope_frequency_base")]
    public double? RopeFrequencyBase { get; set; }

    /// <summary>
    /// Rope frequency scale
    /// </summary>
    [JsonPropertyName("rope_frequency_scale")]
    public double? RopeFrequencyScale { get; set; }

    /// <summary>
    /// Mul mat q
    /// </summary>
    [JsonPropertyName("mul_mat_q")]
    public bool? MulMatQ { get; set; }

    /// <summary>
    /// Rope scaling type
    /// </summary>
    [JsonPropertyName("rope_scaling_type")]
    public string? RopeScalingType { get; set; }
}

/// <summary>
/// Ollama response model
/// </summary>
public class OllamaResponse
{
    /// <summary>
    /// Generated text
    /// </summary>
    [JsonPropertyName("response")]
    public string Response { get; set; } = string.Empty;

    /// <summary>
    /// Context for conversation continuation
    /// </summary>
    [JsonPropertyName("context")]
    public long[]? Context { get; set; }

    /// <summary>
    /// Whether the response is done
    /// </summary>
    [JsonPropertyName("done")]
    public bool Done { get; set; }

    /// <summary>
    /// Total duration in nanoseconds
    /// </summary>
    [JsonPropertyName("total_duration")]
    public long? TotalDuration { get; set; }

    /// <summary>
    /// Load duration in nanoseconds
    /// </summary>
    [JsonPropertyName("load_duration")]
    public long? LoadDuration { get; set; }

    /// <summary>
    /// Prompt evaluation count
    /// </summary>
    [JsonPropertyName("prompt_eval_count")]
    public int? PromptEvalCount { get; set; }

    /// <summary>
    /// Prompt evaluation duration in nanoseconds
    /// </summary>
    [JsonPropertyName("prompt_eval_duration")]
    public long? PromptEvalDuration { get; set; }

    /// <summary>
    /// Evaluation count
    /// </summary>
    [JsonPropertyName("eval_count")]
    public int? EvalCount { get; set; }

    /// <summary>
    /// Evaluation duration in nanoseconds
    /// </summary>
    [JsonPropertyName("eval_duration")]
    public long? EvalDuration { get; set; }

    /// <summary>
    /// Timestamp of the response
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Request ID for tracking
    /// </summary>
    public string RequestId { get; set; } = Guid.NewGuid().ToString();
}

/// <summary>
/// Streaming response from Ollama
/// </summary>
public class OllamaStreamingResponse
{
    /// <summary>
    /// Generated text chunk
    /// </summary>
    [JsonPropertyName("response")]
    public string Response { get; set; } = string.Empty;

    /// <summary>
    /// Context for conversation continuation
    /// </summary>
    [JsonPropertyName("context")]
    public long[]? Context { get; set; }

    /// <summary>
    /// Whether the response is done
    /// </summary>
    [JsonPropertyName("done")]
    public bool Done { get; set; }

    /// <summary>
    /// Timestamp of the chunk
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Request ID for tracking
    /// </summary>
    public string RequestId { get; set; } = Guid.NewGuid().ToString();
}

/// <summary>
/// Ollama model information
/// </summary>
public class OllamaModelInfo
{
    /// <summary>
    /// Model name
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Model size in bytes
    /// </summary>
    [JsonPropertyName("size")]
    public long Size { get; set; }

    /// <summary>
    /// Model digest
    /// </summary>
    [JsonPropertyName("digest")]
    public string Digest { get; set; } = string.Empty;

    /// <summary>
    /// Model details
    /// </summary>
    [JsonPropertyName("details")]
    public OllamaModelDetails? Details { get; set; }

    /// <summary>
    /// Modified timestamp
    /// </summary>
    [JsonPropertyName("modified_at")]
    public DateTime ModifiedAt { get; set; }
}

/// <summary>
/// Detailed model information
/// </summary>
public class OllamaModelDetails
{
    /// <summary>
    /// Format of the model
    /// </summary>
    [JsonPropertyName("format")]
    public string Format { get; set; } = string.Empty;

    /// <summary>
    /// Family of the model
    /// </summary>
    [JsonPropertyName("family")]
    public string Family { get; set; } = string.Empty;

    /// <summary>
    /// Parameter count
    /// </summary>
    [JsonPropertyName("parameter_size")]
    public string ParameterSize { get; set; } = string.Empty;

    /// <summary>
    /// Quantization level
    /// </summary>
    [JsonPropertyName("quantization_level")]
    public string QuantizationLevel { get; set; } = string.Empty;
}

/// <summary>
/// Ollama capabilities
/// </summary>
public class OllamaCapabilities
{
    /// <summary>
    /// Supports text generation
    /// </summary>
    public bool SupportsTextGeneration { get; set; } = true;

    /// <summary>
    /// Supports function calling
    /// </summary>
    public bool SupportsFunctionCalling { get; set; } = true;

    /// <summary>
    /// Supports streaming
    /// </summary>
    public bool SupportsStreaming { get; set; } = true;

    /// <summary>
    /// Supports embeddings
    /// </summary>
    public bool SupportsEmbeddings { get; set; } = true;

    /// <summary>
    /// Supports vision
    /// </summary>
    public bool SupportsVision { get; set; } = false;

    /// <summary>
    /// Maximum input tokens
    /// </summary>
    public int MaxInputTokens { get; set; } = 8192;

    /// <summary>
    /// Maximum output tokens
    /// </summary>
    public int MaxOutputTokens { get; set; } = 4096;

    /// <summary>
    /// Supported models
    /// </summary>
    public List<string> SupportedModels { get; set; } = new()
    {
        "llama3.2",
        "llama3.2:3b",
        "llama3.2:7b",
        "llama3.2:13b",
        "llama3.2:70b",
        "mistral",
        "mistral:7b",
        "mistral:7b-instruct",
        "codellama",
        "codellama:7b",
        "codellama:13b",
        "codellama:34b",
        "phi3",
        "phi3:mini",
        "phi3:small",
        "phi3:medium",
        "gemma2",
        "gemma2:2b",
        "gemma2:7b",
        "gemma2:9b",
        "gemma2:27b"
    };
} 