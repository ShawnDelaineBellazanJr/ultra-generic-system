using UltraGenericSystem.Models;

namespace UltraGenericSystem.Services;

/// <summary>
/// Interface for SK Memory and RAG services
/// </summary>
public interface IMemoryService
{
    /// <summary>
    /// Stores a memory entry
    /// </summary>
    Task<MemoryOperationResult> StoreMemoryAsync(MemoryEntry entry, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Retrieves a memory entry by key
    /// </summary>
    Task<MemoryEntry?> GetMemoryAsync(string key, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Searches memory entries by similarity
    /// </summary>
    Task<MemorySearchResponse> SearchMemoryAsync(MemorySearchRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes a memory entry
    /// </summary>
    Task<MemoryOperationResult> DeleteMemoryAsync(string key, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Stores a whiteboard memory entry
    /// </summary>
    Task<MemoryOperationResult> StoreWhiteboardAsync(WhiteboardMemory entry, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets whiteboard entries for a session
    /// </summary>
    Task<List<WhiteboardMemory>> GetWhiteboardAsync(string sessionId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Clears whiteboard entries for a session
    /// </summary>
    Task<MemoryOperationResult> ClearWhiteboardAsync(string sessionId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Processes a document for RAG
    /// </summary>
    Task<MemoryOperationResult> ProcessDocumentAsync(Document document, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Performs a RAG query
    /// </summary>
    Task<RAGQueryResult> QueryRAGAsync(RAGRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Generates embeddings for text
    /// </summary>
    Task<List<float>> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Calculates similarity between two embeddings
    /// </summary>
    Task<double> CalculateSimilarityAsync(List<float> embedding1, List<float> embedding2, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets memory statistics
    /// </summary>
    Task<Dictionary<string, object>> GetMemoryStatsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Cleans up old memory entries
    /// </summary>
    Task<MemoryOperationResult> CleanupMemoryAsync(TimeSpan olderThan, CancellationToken cancellationToken = default);
} 