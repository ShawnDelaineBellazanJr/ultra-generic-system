using Microsoft.AspNetCore.Mvc;
using UltraGenericSystem.Models;
using UltraGenericSystem.Services;

namespace UltraGenericSystem.Controllers;

/// <summary>
/// Controller for SK Memory and RAG operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MemoryController : ControllerBase
{
    private readonly IMemoryService _memoryService;
    private readonly ILogger<MemoryController> _logger;

    public MemoryController(IMemoryService memoryService, ILogger<MemoryController> logger)
    {
        _memoryService = memoryService;
        _logger = logger;
    }

    /// <summary>
    /// Stores a memory entry
    /// </summary>
    [HttpPost("store")]
    public async Task<ActionResult<MemoryOperationResult>> StoreMemory([FromBody] MemoryEntry entry, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Storing memory entry: {Key}", entry.Key);
            var result = await _memoryService.StoreMemoryAsync(entry, cancellationToken);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing memory entry");
            return StatusCode(500, new MemoryOperationResult
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    /// <summary>
    /// Retrieves a memory entry by key
    /// </summary>
    [HttpGet("get/{key}")]
    public async Task<ActionResult<MemoryEntry>> GetMemory(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving memory entry: {Key}", key);
            var entry = await _memoryService.GetMemoryAsync(key, cancellationToken);
            
            if (entry == null)
            {
                return NotFound();
            }
            
            return Ok(entry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving memory entry: {Key}", key);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Searches memory entries by similarity
    /// </summary>
    [HttpPost("search")]
    public async Task<ActionResult<MemorySearchResponse>> SearchMemory([FromBody] MemorySearchRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Searching memory with query: {Query}", request.Query);
            var result = await _memoryService.SearchMemoryAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching memory with query: {Query}", request.Query);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Deletes a memory entry
    /// </summary>
    [HttpDelete("delete/{key}")]
    public async Task<ActionResult<MemoryOperationResult>> DeleteMemory(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting memory entry: {Key}", key);
            var result = await _memoryService.DeleteMemoryAsync(key, cancellationToken);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting memory entry: {Key}", key);
            return StatusCode(500, new MemoryOperationResult
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    /// <summary>
    /// Stores a whiteboard memory entry
    /// </summary>
    [HttpPost("whiteboard/store")]
    public async Task<ActionResult<MemoryOperationResult>> StoreWhiteboard([FromBody] WhiteboardMemory entry, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Storing whiteboard entry for session: {SessionId}", entry.SessionId);
            var result = await _memoryService.StoreWhiteboardAsync(entry, cancellationToken);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing whiteboard entry");
            return StatusCode(500, new MemoryOperationResult
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    /// <summary>
    /// Gets whiteboard entries for a session
    /// </summary>
    [HttpGet("whiteboard/{sessionId}")]
    public async Task<ActionResult<List<WhiteboardMemory>>> GetWhiteboard(string sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving whiteboard entries for session: {SessionId}", sessionId);
            var entries = await _memoryService.GetWhiteboardAsync(sessionId, cancellationToken);
            return Ok(entries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving whiteboard entries for session: {SessionId}", sessionId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Clears whiteboard entries for a session
    /// </summary>
    [HttpDelete("whiteboard/{sessionId}")]
    public async Task<ActionResult<MemoryOperationResult>> ClearWhiteboard(string sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Clearing whiteboard entries for session: {SessionId}", sessionId);
            var result = await _memoryService.ClearWhiteboardAsync(sessionId, cancellationToken);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing whiteboard entries for session: {SessionId}", sessionId);
            return StatusCode(500, new MemoryOperationResult
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    /// <summary>
    /// Processes a document for RAG
    /// </summary>
    [HttpPost("rag/process-document")]
    public async Task<ActionResult<MemoryOperationResult>> ProcessDocument([FromBody] Document document, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing document for RAG: {Title}", document.Title);
            var result = await _memoryService.ProcessDocumentAsync(document, cancellationToken);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing document: {Title}", document.Title);
            return StatusCode(500, new MemoryOperationResult
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    /// <summary>
    /// Performs a RAG query
    /// </summary>
    [HttpPost("rag/query")]
    public async Task<ActionResult<RAGQueryResult>> QueryRAG([FromBody] RAGRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Performing RAG query: {Query}", request.Query);
            var result = await _memoryService.QueryRAGAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing RAG query: {Query}", request.Query);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Generates embeddings for text
    /// </summary>
    [HttpPost("embedding/generate")]
    public async Task<ActionResult<List<float>>> GenerateEmbedding([FromBody] string text, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating embedding for text");
            var embedding = await _memoryService.GenerateEmbeddingAsync(text, cancellationToken);
            return Ok(embedding);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embedding for text");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Calculates similarity between two embeddings
    /// </summary>
    [HttpPost("embedding/similarity")]
    public async Task<ActionResult<double>> CalculateSimilarity([FromBody] SimilarityRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Calculating similarity between embeddings");
            var similarity = await _memoryService.CalculateSimilarityAsync(request.Embedding1, request.Embedding2, cancellationToken);
            return Ok(similarity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating similarity between embeddings");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Gets memory statistics
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<Dictionary<string, object>>> GetMemoryStats(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting memory statistics");
            var stats = await _memoryService.GetMemoryStatsAsync(cancellationToken);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting memory statistics");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Cleans up old memory entries
    /// </summary>
    [HttpPost("cleanup")]
    public async Task<ActionResult<MemoryOperationResult>> CleanupMemory([FromBody] CleanupRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Cleaning up memory entries older than {Days} days", request.Days);
            var result = await _memoryService.CleanupMemoryAsync(TimeSpan.FromDays(request.Days), cancellationToken);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up memory entries");
            return StatusCode(500, new MemoryOperationResult
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }
}

/// <summary>
/// Request model for similarity calculation
/// </summary>
public class SimilarityRequest
{
    public List<float> Embedding1 { get; set; } = new();
    public List<float> Embedding2 { get; set; } = new();
}

/// <summary>
/// Request model for memory cleanup
/// </summary>
public class CleanupRequest
{
    public int Days { get; set; } = 30;
} 