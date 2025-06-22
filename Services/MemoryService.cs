using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.TextGeneration;
using UltraGenericSystem.Models;
using UltraGenericSystem.Repositories;

namespace UltraGenericSystem.Services;

/// <summary>
/// Implementation of SK Memory and RAG services
/// </summary>
public class MemoryService : IMemoryService
{
    private readonly Kernel _kernel;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MemoryService> _logger;
    private readonly MemoryConfig _config;
    private readonly Dictionary<string, List<MemoryEntry>> _memoryCache;
    private readonly Dictionary<string, List<WhiteboardMemory>> _whiteboardCache;

    public MemoryService(
        Kernel kernel,
        IUnitOfWork unitOfWork,
        ILogger<MemoryService> logger,
        MemoryConfig config)
    {
        _kernel = kernel;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _config = config;
        _memoryCache = new Dictionary<string, List<MemoryEntry>>();
        _whiteboardCache = new Dictionary<string, List<WhiteboardMemory>>();
    }

    /// <summary>
    /// Stores a memory entry
    /// </summary>
    public async Task<MemoryOperationResult> StoreMemoryAsync(MemoryEntry entry, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Storing memory entry: {Key}", entry.Key);

            // Generate embedding if not provided
            if (entry.Embedding == null || !entry.Embedding.Any())
            {
                entry.Embedding = await GenerateEmbeddingAsync(entry.Content, cancellationToken);
            }

            // Store in database
            var repository = _unitOfWork.Repository<MemoryEntry>();
            await repository.CreateAsync(entry, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Update cache
            var collection = entry.Collection ?? "default";
            if (!_memoryCache.ContainsKey(collection))
            {
                _memoryCache[collection] = new List<MemoryEntry>();
            }
            _memoryCache[collection].Add(entry);

            _logger.LogInformation("Successfully stored memory entry: {Key}", entry.Key);
            return new MemoryOperationResult
            {
                Success = true,
                Message = "Memory entry stored successfully",
                AffectedCount = 1
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing memory entry: {Key}", entry.Key);
            return new MemoryOperationResult
            {
                Success = false,
                Message = $"Failed to store memory entry: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Retrieves a memory entry by key
    /// </summary>
    public async Task<MemoryEntry?> GetMemoryAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving memory entry: {Key}", key);

            var repository = _unitOfWork.Repository<MemoryEntry>();
            var entries = await repository.QueryAsync(e => e.Key == key, cancellationToken);
            var entry = entries.FirstOrDefault();

            if (entry != null)
            {
                // Update access statistics
                entry.LastAccessed = DateTime.UtcNow;
                entry.AccessCount++;
                await repository.UpdateAsync(entry, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return entry;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving memory entry: {Key}", key);
            return null;
        }
    }

    /// <summary>
    /// Searches memory entries by similarity
    /// </summary>
    public async Task<MemorySearchResponse> SearchMemoryAsync(MemorySearchRequest request, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        var results = new List<MemoryEntry>();

        try
        {
            _logger.LogInformation("Searching memory with query: {Query}", request.Query);

            // Generate embedding for query
            var queryEmbedding = await GenerateEmbeddingAsync(request.Query, cancellationToken);

            // Get all memory entries from the collection
            var repository = _unitOfWork.Repository<MemoryEntry>();
            var collection = request.Collection ?? "default";
            var allEntries = await repository.QueryAsync(e => e.Collection == collection, cancellationToken);

            // Calculate similarities and filter results
            foreach (var entry in allEntries)
            {
                if (entry.Embedding != null && entry.Embedding.Any())
                {
                    var similarity = await CalculateSimilarityAsync(queryEmbedding, entry.Embedding, cancellationToken);
                    if (similarity >= request.MinSimilarity)
                    {
                        entry.SimilarityScore = similarity;
                        results.Add(entry);
                    }
                }
            }

            // Sort by similarity and limit results
            results = results
                .OrderByDescending(r => r.SimilarityScore)
                .Take(request.MaxResults)
                .ToList();

            var searchTime = DateTime.UtcNow - startTime;
            _logger.LogInformation("Memory search completed in {SearchTime}ms with {ResultCount} results", 
                searchTime.TotalMilliseconds, results.Count);

            return new MemorySearchResponse
            {
                Results = results,
                TotalCount = results.Count,
                SearchTime = searchTime,
                Metadata = new Dictionary<string, object>
                {
                    ["query"] = request.Query,
                    ["collection"] = collection,
                    ["minSimilarity"] = request.MinSimilarity
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching memory with query: {Query}", request.Query);
            return new MemorySearchResponse
            {
                Results = new List<MemoryEntry>(),
                TotalCount = 0,
                SearchTime = DateTime.UtcNow - startTime
            };
        }
    }

    /// <summary>
    /// Deletes a memory entry
    /// </summary>
    public async Task<MemoryOperationResult> DeleteMemoryAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting memory entry: {Key}", key);

            var repository = _unitOfWork.Repository<MemoryEntry>();
            var entries = await repository.QueryAsync(e => e.Key == key, cancellationToken);
            var entry = entries.FirstOrDefault();

            if (entry != null)
            {
                await repository.DeleteAsync(entry.Id, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Remove from cache
                var collection = entry.Collection ?? "default";
                if (_memoryCache.ContainsKey(collection))
                {
                    _memoryCache[collection].RemoveAll(e => e.Key == key);
                }

                _logger.LogInformation("Successfully deleted memory entry: {Key}", key);
                return new MemoryOperationResult
                {
                    Success = true,
                    Message = "Memory entry deleted successfully",
                    AffectedCount = 1
                };
            }

            return new MemoryOperationResult
            {
                Success = false,
                Message = "Memory entry not found"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting memory entry: {Key}", key);
            return new MemoryOperationResult
            {
                Success = false,
                Message = $"Failed to delete memory entry: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Stores a whiteboard memory entry
    /// </summary>
    public async Task<MemoryOperationResult> StoreWhiteboardAsync(WhiteboardMemory entry, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Storing whiteboard entry for session: {SessionId}", entry.SessionId);

            // Store in database
            var repository = _unitOfWork.Repository<WhiteboardMemory>();
            await repository.CreateAsync(entry, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Update cache
            if (!_whiteboardCache.ContainsKey(entry.SessionId))
            {
                _whiteboardCache[entry.SessionId] = new List<WhiteboardMemory>();
            }
            _whiteboardCache[entry.SessionId].Add(entry);

            // Limit cache size
            if (_whiteboardCache[entry.SessionId].Count > _config.MaxWhiteboardEntries)
            {
                var oldestEntry = _whiteboardCache[entry.SessionId].OrderBy(e => e.Timestamp).First();
                _whiteboardCache[entry.SessionId].Remove(oldestEntry);
            }

            return new MemoryOperationResult
            {
                Success = true,
                Message = "Whiteboard entry stored successfully",
                AffectedCount = 1
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing whiteboard entry for session: {SessionId}", entry.SessionId);
            return new MemoryOperationResult
            {
                Success = false,
                Message = $"Failed to store whiteboard entry: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Gets whiteboard entries for a session
    /// </summary>
    public async Task<List<WhiteboardMemory>> GetWhiteboardAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving whiteboard entries for session: {SessionId}", sessionId);

            // Check cache first
            if (_whiteboardCache.ContainsKey(sessionId))
            {
                return _whiteboardCache[sessionId].OrderBy(e => e.Timestamp).ToList();
            }

            // Get from database
            var repository = _unitOfWork.Repository<WhiteboardMemory>();
            var entries = await repository.QueryAsync(e => e.SessionId == sessionId, cancellationToken);

            // Update cache
            _whiteboardCache[sessionId] = entries.OrderBy(e => e.Timestamp).ToList();

            return _whiteboardCache[sessionId];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving whiteboard entries for session: {SessionId}", sessionId);
            return new List<WhiteboardMemory>();
        }
    }

    /// <summary>
    /// Clears whiteboard entries for a session
    /// </summary>
    public async Task<MemoryOperationResult> ClearWhiteboardAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Clearing whiteboard entries for session: {SessionId}", sessionId);

            var repository = _unitOfWork.Repository<WhiteboardMemory>();
            var entries = await repository.QueryAsync(e => e.SessionId == sessionId, cancellationToken);

            foreach (var entry in entries)
            {
                await repository.DeleteAsync(entry.Id, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Clear cache
            if (_whiteboardCache.ContainsKey(sessionId))
            {
                _whiteboardCache.Remove(sessionId);
            }

            return new MemoryOperationResult
            {
                Success = true,
                Message = "Whiteboard entries cleared successfully",
                AffectedCount = entries.Count()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing whiteboard entries for session: {SessionId}", sessionId);
            return new MemoryOperationResult
            {
                Success = false,
                Message = $"Failed to clear whiteboard entries: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Processes a document for RAG
    /// </summary>
    public async Task<MemoryOperationResult> ProcessDocumentAsync(Document document, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing document for RAG: {Title}", document.Title);

            // Chunk the document
            var chunks = ChunkDocument(document.Content);
            document.Chunks = chunks.Select((content, index) => new DocumentChunk
            {
                Content = content,
                ChunkIndex = index,
                StartPosition = index * 1000, // Approximate
                EndPosition = (index + 1) * 1000,
                DocumentId = document.Id
            }).ToList();

            // Generate embeddings for chunks
            foreach (var chunk in document.Chunks)
            {
                chunk.Embedding = await GenerateEmbeddingAsync(chunk.Content, cancellationToken);
            }

            // Store document and chunks
            var docRepository = _unitOfWork.Repository<Document>();
            var chunkRepository = _unitOfWork.Repository<DocumentChunk>();

            await docRepository.CreateAsync(document, cancellationToken);
            
            // Store chunks individually since there's no AddRangeAsync
            foreach (var chunk in document.Chunks)
            {
                await chunkRepository.CreateAsync(chunk, cancellationToken);
            }
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            document.IsProcessed = true;
            document.ProcessedAt = DateTime.UtcNow;

            _logger.LogInformation("Successfully processed document: {Title} with {ChunkCount} chunks", 
                document.Title, document.Chunks.Count);

            return new MemoryOperationResult
            {
                Success = true,
                Message = "Document processed successfully",
                AffectedCount = document.Chunks.Count + 1
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing document: {Title}", document.Title);
            return new MemoryOperationResult
            {
                Success = false,
                Message = $"Failed to process document: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Performs a RAG query
    /// </summary>
    public async Task<RAGQueryResult> QueryRAGAsync(RAGRequest request, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            _logger.LogInformation("Performing RAG query: {Query}", request.Query);

            // Generate embedding for query
            var queryEmbedding = await GenerateEmbeddingAsync(request.Query, cancellationToken);

            // Search document chunks
            var chunkRepository = _unitOfWork.Repository<DocumentChunk>();
            var allChunks = await chunkRepository.GetAllAsync(cancellationToken);

            var matches = new List<RAGMatch>();

            foreach (var chunk in allChunks)
            {
                if (chunk.Embedding != null && chunk.Embedding.Any())
                {
                    var similarity = await CalculateSimilarityAsync(queryEmbedding, chunk.Embedding, cancellationToken);
                    if (similarity >= request.MinSimilarity)
                    {
                        matches.Add(new RAGMatch
                        {
                            Content = chunk.Content,
                            SimilarityScore = similarity,
                            DocumentId = chunk.DocumentId,
                            ChunkIndex = chunk.ChunkIndex
                        });
                    }
                }
            }

            // Sort by similarity and limit results
            matches = matches
                .OrderByDescending(m => m.SimilarityScore)
                .Take(request.MaxResults)
                .ToList();

            string generatedResponse = string.Empty;
            if (request.GenerateResponse && matches.Any())
            {
                generatedResponse = await GenerateRAGResponseAsync(request.Query, matches, cancellationToken);
            }

            var queryTime = DateTime.UtcNow - startTime;

            return new RAGQueryResult
            {
                Query = request.Query,
                Matches = matches,
                GeneratedResponse = generatedResponse,
                Sources = matches.Select(m => m.Content).ToList(),
                QueryTime = queryTime,
                QueryTimestamp = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    ["matchCount"] = matches.Count,
                    ["minSimilarity"] = request.MinSimilarity,
                    ["maxResults"] = request.MaxResults
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing RAG query: {Query}", request.Query);
            return new RAGQueryResult
            {
                Query = request.Query,
                Matches = new List<RAGMatch>(),
                GeneratedResponse = string.Empty,
                QueryTime = DateTime.UtcNow - startTime,
                QueryTimestamp = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Generates embeddings for text
    /// </summary>
    public async Task<List<float>> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        try
        {
            // For now, we'll use a simple hash-based embedding
            // In a real implementation, you would use an embedding model
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

            return embedding;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embedding for text");
            return new List<float>();
        }
    }

    /// <summary>
    /// Calculates similarity between two embeddings
    /// </summary>
    public async Task<double> CalculateSimilarityAsync(List<float> embedding1, List<float> embedding2, CancellationToken cancellationToken = default)
    {
        try
        {
            if (embedding1.Count != embedding2.Count)
            {
                return 0.0;
            }

            // Calculate cosine similarity
            double dotProduct = 0;
            double magnitude1 = 0;
            double magnitude2 = 0;

            for (int i = 0; i < embedding1.Count; i++)
            {
                dotProduct += embedding1[i] * embedding2[i];
                magnitude1 += embedding1[i] * embedding1[i];
                magnitude2 += embedding2[i] * embedding2[i];
            }

            magnitude1 = Math.Sqrt(magnitude1);
            magnitude2 = Math.Sqrt(magnitude2);

            if (magnitude1 == 0 || magnitude2 == 0)
            {
                return 0.0;
            }

            return dotProduct / (magnitude1 * magnitude2);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating similarity between embeddings");
            return 0.0;
        }
    }

    /// <summary>
    /// Gets memory statistics
    /// </summary>
    public async Task<Dictionary<string, object>> GetMemoryStatsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var memoryRepo = _unitOfWork.Repository<MemoryEntry>();
            var whiteboardRepo = _unitOfWork.Repository<WhiteboardMemory>();
            var documentRepo = _unitOfWork.Repository<Document>();
            var chunkRepo = _unitOfWork.Repository<DocumentChunk>();

            var memoryCount = await memoryRepo.CountAsync(cancellationToken);
            var whiteboardCount = await whiteboardRepo.CountAsync(cancellationToken);
            var documentCount = await documentRepo.CountAsync(cancellationToken);
            var chunkCount = await chunkRepo.CountAsync(cancellationToken);

            return new Dictionary<string, object>
            {
                ["memoryEntries"] = memoryCount,
                ["whiteboardEntries"] = whiteboardCount,
                ["documents"] = documentCount,
                ["documentChunks"] = chunkCount,
                ["cacheSize"] = _memoryCache.Values.Sum(c => c.Count),
                ["whiteboardCacheSize"] = _whiteboardCache.Values.Sum(c => c.Count)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting memory statistics");
            return new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Cleans up old memory entries
    /// </summary>
    public async Task<MemoryOperationResult> CleanupMemoryAsync(TimeSpan olderThan, CancellationToken cancellationToken = default)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow - olderThan;
            var repository = _unitOfWork.Repository<MemoryEntry>();
            var oldEntries = await repository.QueryAsync(e => e.CreatedAt < cutoffDate, cancellationToken);

            foreach (var entry in oldEntries)
            {
                await repository.DeleteAsync(entry.Id, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new MemoryOperationResult
            {
                Success = true,
                Message = $"Cleaned up {oldEntries.Count()} old memory entries",
                AffectedCount = oldEntries.Count()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up old memory entries");
            return new MemoryOperationResult
            {
                Success = false,
                Message = $"Failed to cleanup memory: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Chunks a document into smaller pieces
    /// </summary>
    private List<string> ChunkDocument(string content, int chunkSize = 1000)
    {
        var chunks = new List<string>();
        var words = content.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var currentChunk = new List<string>();

        foreach (var word in words)
        {
            currentChunk.Add(word);
            if (currentChunk.Count >= chunkSize)
            {
                chunks.Add(string.Join(" ", currentChunk));
                currentChunk.Clear();
            }
        }

        if (currentChunk.Any())
        {
            chunks.Add(string.Join(" ", currentChunk));
        }

        return chunks;
    }

    /// <summary>
    /// Generates a RAG response using the matches
    /// </summary>
    private async Task<string> GenerateRAGResponseAsync(string query, List<RAGMatch> matches, CancellationToken cancellationToken = default)
    {
        try
        {
            var context = string.Join("\n\n", matches.Select(m => m.Content));
            var prompt = $@"
Based on the following context, answer the question. If the context doesn't contain enough information to answer the question, say so.

Context:
{context}

Question: {query}

Answer:";

            // Use the kernel directly with a simple prompt
            var result = await _kernel.InvokePromptAsync(prompt, cancellationToken: cancellationToken);
            return result.GetValue<string>() ?? "Unable to generate response.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating RAG response");
            return "Error generating response.";
        }
    }
} 