using System.ComponentModel.DataAnnotations;

namespace UltraGenericSystem.Models;

/// <summary>
/// Memory entry for storing information in the SK memory system
/// </summary>
public class MemoryEntry : BaseEntity
{
    [Required]
    [MaxLength(500)]
    public string Key { get; set; } = string.Empty;
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public new string? Description { get; set; }
    
    [MaxLength(100)]
    public string? Source { get; set; }
    
    [MaxLength(100)]
    public string? Collection { get; set; }
    
    public List<float>? Embedding { get; set; }
    
    public double? SimilarityScore { get; set; }
    
    public DateTime LastAccessed { get; set; } = DateTime.UtcNow;
    
    public int AccessCount { get; set; } = 0;
}

/// <summary>
/// Whiteboard memory for agent collaboration
/// </summary>
public class WhiteboardMemory : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string SessionId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string AgentId { get; set; } = string.Empty;
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? MessageType { get; set; }
    
    public Dictionary<string, object>? Context { get; set; }
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    public bool IsShared { get; set; } = false;
}

/// <summary>
/// Vector embedding for similarity search
/// </summary>
public class VectorEmbedding : BaseEntity
{
    [Required]
    public string Text { get; set; } = string.Empty;
    
    [Required]
    public List<float> Embedding { get; set; } = new();
    
    [MaxLength(100)]
    public string? Model { get; set; }
    
    [MaxLength(100)]
    public string? Collection { get; set; }
    
    public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Document for RAG processing
/// </summary>
public class Document : BaseEntity
{
    [Required]
    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? Type { get; set; }
    
    [MaxLength(100)]
    public string? Source { get; set; }
    
    [MaxLength(1000)]
    public string? Url { get; set; }
    
    public List<DocumentChunk> Chunks { get; set; } = new();
    
    public bool IsProcessed { get; set; } = false;
    
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Document chunk for RAG processing
/// </summary>
public class DocumentChunk : BaseEntity
{
    [Required]
    public string Content { get; set; } = string.Empty;
    
    public int ChunkIndex { get; set; }
    
    public int StartPosition { get; set; }
    
    public int EndPosition { get; set; }
    
    public List<float>? Embedding { get; set; }
    
    public Guid DocumentId { get; set; }
    
    public Document? Document { get; set; }
}

/// <summary>
/// RAG query result
/// </summary>
public class RAGQueryResult
{
    public string Query { get; set; } = string.Empty;
    
    public List<RAGMatch> Matches { get; set; } = new();
    
    public string GeneratedResponse { get; set; } = string.Empty;
    
    public List<string> Sources { get; set; } = new();
    
    public Dictionary<string, object>? Metadata { get; set; }
    
    public TimeSpan QueryTime { get; set; }
    
    public DateTime QueryTimestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// RAG match from similarity search
/// </summary>
public class RAGMatch
{
    public string Content { get; set; } = string.Empty;
    
    public double SimilarityScore { get; set; }
    
    public string? Source { get; set; }
    
    public Dictionary<string, object>? Metadata { get; set; }
    
    public Guid? DocumentId { get; set; }
    
    public int? ChunkIndex { get; set; }
}

/// <summary>
/// Memory configuration
/// </summary>
public class MemoryConfig
{
    public bool EnableMem0 { get; set; } = true;
    
    public bool EnableWhiteboard { get; set; } = true;
    
    public bool EnableRAG { get; set; } = true;
    
    public int MaxMemoryEntries { get; set; } = 10000;
    
    public int MaxWhiteboardEntries { get; set; } = 1000;
    
    public int MaxRAGResults { get; set; } = 10;
    
    public double MinSimilarityThreshold { get; set; } = 0.7;
    
    public string? EmbeddingModel { get; set; }
    
    public string? VectorStoreType { get; set; }
    
    public Dictionary<string, object>? VectorStoreConfig { get; set; }
}

/// <summary>
/// Memory search request
/// </summary>
public class MemorySearchRequest
{
    public string Query { get; set; } = string.Empty;
    
    public string? Collection { get; set; }
    
    public int MaxResults { get; set; } = 10;
    
    public double MinSimilarity { get; set; } = 0.7;
    
    public Dictionary<string, object>? Filters { get; set; }
    
    public bool IncludeMetadata { get; set; } = true;
}

/// <summary>
/// Memory search response
/// </summary>
public class MemorySearchResponse
{
    public List<MemoryEntry> Results { get; set; } = new();
    
    public int TotalCount { get; set; }
    
    public TimeSpan SearchTime { get; set; }
    
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// RAG request
/// </summary>
public class RAGRequest
{
    public string Query { get; set; } = string.Empty;
    
    public string? Context { get; set; }
    
    public int MaxResults { get; set; } = 5;
    
    public double MinSimilarity { get; set; } = 0.7;
    
    public bool GenerateResponse { get; set; } = true;
    
    public Dictionary<string, object>? Parameters { get; set; }
}

/// <summary>
/// Memory operation result
/// </summary>
public class MemoryOperationResult
{
    public bool Success { get; set; }
    
    public string? Message { get; set; }
    
    public int AffectedCount { get; set; }
    
    public Dictionary<string, object>? Metadata { get; set; }
} 