using Microsoft.AspNetCore.Mvc;
using UltraGenericSystem.Models;
using UltraGenericSystem.Services;

namespace UltraGenericSystem.Controllers;

/// <summary>
/// Model Context Protocol (MCP) Controller
/// Enables self-evolving system to discover and auto-generate SK plugins from MCP schemas
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MCPController : ControllerBase
{
    private readonly IMCPService _mcpService;
    private readonly ILogger<MCPController> _logger;

    public MCPController(IMCPService mcpService, ILogger<MCPController> logger)
    {
        _mcpService = mcpService;
        _logger = logger;
    }

    /// <summary>
    /// Discover and connect to GitHub MCP server
    /// </summary>
    [HttpPost("discover-github")]
    public async Task<ActionResult<MCPDiscoveryResult>> DiscoverGitHubMCPServer([FromBody] GitHubDiscoveryRequest request)
    {
        try
        {
            _logger.LogInformation("Discovering GitHub MCP server");
            
            var result = await _mcpService.DiscoverGitHubMCPServerAsync(request.GitHubToken);
            
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to discover GitHub MCP server");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Discover MCP server capabilities
    /// </summary>
    [HttpPost("discover-capabilities")]
    public async Task<ActionResult<MCPDiscoveryResult>> DiscoverMCPServerCapabilities([FromBody] MCPServer server)
    {
        try
        {
            _logger.LogInformation("Discovering MCP server capabilities for {ServerName}", server.Name);
            
            var result = await _mcpService.DiscoverMCPServerCapabilitiesAsync(server);
            
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to discover MCP server capabilities");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Trigger self-evolving plugin generation from MCP schema
    /// </summary>
    [HttpPost("generate-plugins")]
    public async Task<ActionResult<PluginGenerationResult>> GeneratePluginsFromMCP([FromBody] PluginGenerationRequest request)
    {
        try
        {
            _logger.LogInformation("Triggering self-evolving plugin generation from MCP schema");
            
            var result = await _mcpService.TriggerSelfEvolvingPluginGenerationAsync(request.Server, request.Discovery);
            
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate plugins from MCP");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Execute GitHub operation through MCP
    /// </summary>
    [HttpPost("github/execute")]
    public async Task<ActionResult<GitHubOperationResult>> ExecuteGitHubOperation([FromBody] GitHubOperationRequest request)
    {
        try
        {
            _logger.LogInformation("Executing GitHub operation: {Operation}", request.Operation);
            
            var result = await _mcpService.ExecuteGitHubOperationAsync(request.Operation, request.Parameters);
            
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute GitHub operation");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get repository information through GitHub MCP
    /// </summary>
    [HttpGet("github/repository/{owner}/{repo}")]
    public async Task<ActionResult<GitHubRepositoryInfo>> GetRepositoryInfo(string owner, string repo)
    {
        try
        {
            _logger.LogInformation("Getting repository info for {Owner}/{Repo}", owner, repo);
            
            var result = await _mcpService.GetRepositoryInfoAsync(owner, repo);
            
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get repository info");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// List issues through GitHub MCP
    /// </summary>
    [HttpGet("github/issues/{owner}/{repo}")]
    public async Task<ActionResult<GitHubIssuesResult>> ListIssues(string owner, string repo, [FromQuery] string? state = "open")
    {
        try
        {
            _logger.LogInformation("Listing issues for {Owner}/{Repo} with state {State}", owner, repo, state);
            
            var result = await _mcpService.ListIssuesAsync(owner, repo, state);
            
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list issues");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Create issue through GitHub MCP
    /// </summary>
    [HttpPost("github/issues/{owner}/{repo}")]
    public async Task<ActionResult<GitHubIssueResult>> CreateIssue(string owner, string repo, [FromBody] CreateIssueRequest request)
    {
        try
        {
            _logger.LogInformation("Creating issue for {Owner}/{Repo}: {Title}", owner, repo, request.Title);
            
            var result = await _mcpService.CreateIssueAsync(owner, repo, request.Title, request.Body);
            
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create issue");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get connected MCP servers
    /// </summary>
    [HttpGet("servers")]
    public ActionResult<Dictionary<string, MCPServer>> GetConnectedServers()
    {
        try
        {
            var servers = _mcpService.GetConnectedServers();
            return Ok(servers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get connected servers");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Disconnect from MCP server
    /// </summary>
    [HttpDelete("servers/{serverName}")]
    public async Task<ActionResult<bool>> DisconnectServer(string serverName)
    {
        try
        {
            _logger.LogInformation("Disconnecting from MCP server: {ServerName}", serverName);
            
            var result = await _mcpService.DisconnectServerAsync(serverName);
            
            if (result)
            {
                return Ok(new { success = true, message = $"Disconnected from {serverName}" });
            }
            else
            {
                return BadRequest(new { success = false, message = $"Failed to disconnect from {serverName}" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to disconnect from MCP server");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get MCP self-evolution context
    /// </summary>
    [HttpGet("evolution-context")]
    public ActionResult<MCPSelfEvolutionContext> GetSelfEvolutionContext()
    {
        try
        {
            var servers = _mcpService.GetConnectedServers();
            
            var context = new MCPSelfEvolutionContext
            {
                ConnectedServers = servers.Values.ToList(),
                Timestamp = DateTime.UtcNow
            };
            
            return Ok(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get self-evolution context");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Test MCP connection
    /// </summary>
    [HttpPost("test-connection")]
    public async Task<ActionResult<MCPTestResult>> TestConnection([FromBody] MCPTestRequest request)
    {
        try
        {
            _logger.LogInformation("Testing MCP connection to {Endpoint}", request.Endpoint);
            
            var testServer = new MCPServer
            {
                Name = request.Name ?? "Test Server",
                Endpoint = request.Endpoint,
                Authentication = request.Authentication
            };
            
            var result = await _mcpService.DiscoverMCPServerCapabilitiesAsync(testServer);
            
            var testResult = new MCPTestResult
            {
                Success = result.Success,
                Endpoint = request.Endpoint,
                CapabilitiesCount = result.Capabilities.Count,
                ErrorMessage = result.ErrorMessage,
                Timestamp = DateTime.UtcNow
            };
            
            if (result.Success)
            {
                return Ok(testResult);
            }
            else
            {
                return BadRequest(testResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to test MCP connection");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

/// <summary>
/// GitHub Discovery Request
/// </summary>
public class GitHubDiscoveryRequest
{
    /// <summary>
    /// GitHub token for authentication
    /// </summary>
    public string? GitHubToken { get; set; }
}

/// <summary>
/// GitHub Operation Request
/// </summary>
public class GitHubOperationRequest
{
    /// <summary>
    /// Operation name
    /// </summary>
    public string Operation { get; set; } = string.Empty;

    /// <summary>
    /// Operation parameters
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new();
}

/// <summary>
/// Create Issue Request
/// </summary>
public class CreateIssueRequest
{
    /// <summary>
    /// Issue title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Issue body
    /// </summary>
    public string Body { get; set; } = string.Empty;
}

/// <summary>
/// MCP Test Request
/// </summary>
public class MCPTestRequest
{
    /// <summary>
    /// Server name
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Server endpoint
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Authentication configuration
    /// </summary>
    public MCPAuth? Authentication { get; set; }
}

/// <summary>
/// MCP Test Result
/// </summary>
public class MCPTestResult
{
    /// <summary>
    /// Whether test was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Tested endpoint
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Number of discovered capabilities
    /// </summary>
    public int CapabilitiesCount { get; set; }

    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Test timestamp
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
} 