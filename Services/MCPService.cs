using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using System.Text.Json;
using UltraGenericSystem.Models;
using UltraGenericSystem.Services.Agents;
using Spectre.Console;

namespace UltraGenericSystem.Services;

/// <summary>
/// Model Context Protocol (MCP) Service for GitHub API integration
/// Enables self-evolving system to discover and auto-generate SK plugins from MCP schemas
/// </summary>
public class MCPService : IMCPService
{
    private readonly ILogger<MCPService> _logger;
    private readonly IConversationalLogger _conversationalLogger;
    private readonly IAgentOrchestrator _agentOrchestrator;
    private readonly Kernel _kernel;
    private readonly HttpClient _httpClient;
    private readonly Dictionary<string, MCPServer> _mcpServers;

    public MCPService(
        ILogger<MCPService> logger,
        IConversationalLogger conversationalLogger,
        IAgentOrchestrator agentOrchestrator,
        Kernel kernel,
        HttpClient httpClient)
    {
        _logger = logger;
        _conversationalLogger = conversationalLogger;
        _agentOrchestrator = agentOrchestrator;
        _kernel = kernel;
        _httpClient = httpClient;
        _mcpServers = new Dictionary<string, MCPServer>();
        
        _logger.LogInformation("MCPService initialized for self-evolving MCP integration");
        _conversationalLogger.LogSystemMessage("MCPService ready for MCP discovery and plugin generation", Color.Green);
    }

    /// <summary>
    /// Discover and connect to GitHub MCP server
    /// </summary>
    public async Task<MCPDiscoveryResult> DiscoverGitHubMCPServerAsync(string? githubToken = null)
    {
        try
        {
            _conversationalLogger.LogSystemMessage("🔍 Discovering GitHub MCP server...", Color.Blue);
            
            // GitHub MCP server typically runs on localhost:3000
            var githubMCP = new MCPServer
            {
                Name = "GitHub MCP",
                Endpoint = "http://localhost:3000",
                Capabilities = new List<string> { "github_api", "repositories", "issues", "pull_requests" },
                Authentication = githubToken != null ? new MCPAuth { Type = "github_token", Token = githubToken } : null
            };

            // Test connection and discover capabilities
            var discoveryResult = await DiscoverMCPServerCapabilitiesAsync(githubMCP);
            
            if (discoveryResult.Success)
            {
                _mcpServers["github"] = githubMCP;
                _conversationalLogger.LogSystemMessage($"✅ GitHub MCP server discovered with {discoveryResult.Capabilities.Count} capabilities", Color.Green);
                
                // Trigger self-evolving plugin generation
                await TriggerSelfEvolvingPluginGenerationAsync(githubMCP, discoveryResult);
            }

            return discoveryResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to discover GitHub MCP server");
            _conversationalLogger.LogError($"GitHub MCP discovery failed: {ex.Message}");
            
            return new MCPDiscoveryResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Discover MCP server capabilities and schema
    /// </summary>
    public async Task<MCPDiscoveryResult> DiscoverMCPServerCapabilitiesAsync(MCPServer server)
    {
        try
        {
            _conversationalLogger.LogAgentMessage("MCPDiscovery", "discovery", $"Discovering capabilities for {server.Name}", $"Endpoint: {server.Endpoint}");
            
            // MCP discovery request
            var discoveryRequest = new MCPRequest
            {
                Method = "initialize",
                Params = new Dictionary<string, object>
                {
                    ["protocolVersion"] = "2024-11-05",
                    ["capabilities"] = new Dictionary<string, object>
                    {
                        ["tools"] = new Dictionary<string, object>(),
                        ["notifications"] = new Dictionary<string, object>()
                    },
                    ["clientInfo"] = new Dictionary<string, object>
                    {
                        ["name"] = "UltraGenericSystem",
                        ["version"] = "1.0.0"
                    }
                }
            };

            var response = await SendMCPRequestAsync(server, discoveryRequest);
            
            if (response.Success)
            {
                var capabilities = ParseMCPCapabilities(response.Result);
                
                _conversationalLogger.LogAgentMessage("MCPDiscovery", "discovery", $"Discovered {capabilities.Count} capabilities", 
                    string.Join(", ", capabilities.Select(c => c.Name)));
                
                return new MCPDiscoveryResult
                {
                    Success = true,
                    Server = server,
                    Capabilities = capabilities,
                    Schema = response.Result,
                    Timestamp = DateTime.UtcNow
                };
            }
            else
            {
                return new MCPDiscoveryResult
                {
                    Success = false,
                    ErrorMessage = response.ErrorMessage,
                    Timestamp = DateTime.UtcNow
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to discover MCP server capabilities");
            return new MCPDiscoveryResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Trigger self-evolving plugin generation from MCP schema
    /// </summary>
    public async Task<PluginGenerationResult> TriggerSelfEvolvingPluginGenerationAsync(MCPServer server, MCPDiscoveryResult discovery)
    {
        try
        {
            _conversationalLogger.LogSystemMessage("🧠 Triggering self-evolving plugin generation from MCP schema", Color.Purple);
            
            // Create a self-evolving orchestration request
            var request = new AdvancedAgentRequest<PluginGenerationRequest, PluginGenerationResult>
            {
                Input = new PluginGenerationRequest
                {
                    Server = server,
                    Discovery = discovery,
                    Goal = "Generate SK plugins from MCP schema using self-evolving capabilities",
                    TargetCapabilities = discovery.Capabilities?.Select(c => c.Name).ToList() ?? new List<string>(),
                    Preferences = new Dictionary<string, object>
                    {
                        ["codeGenerationApproach"] = "template_based",
                        ["errorHandlingStrategy"] = "graceful_degradation",
                        ["testingApproach"] = "generative_testing"
                    }
                },
                Operation = "GeneratePluginsFromMCP",
                OrchestrationConfig = new AdvancedOrchestrationConfig
                {
                    EnableResponseCallbacks = true,
                    Timeout = TimeSpan.FromMinutes(10)
                }
            };

            var result = await _agentOrchestrator.ExecuteAdvancedOrchestrationAsync(request);
            
            if (result.Success)
            {
                _conversationalLogger.LogSystemMessage($"✅ Self-evolving plugin generation completed: {result.Output?.GeneratedPlugins?.Count ?? 0} plugins", Color.Green);
            }
            else
            {
                _conversationalLogger.LogError($"Self-evolving plugin generation failed: {result.ErrorMessage}");
            }

            return result.Output ?? new PluginGenerationResult
            {
                Success = result.Success,
                ErrorMessage = result.ErrorMessage,
                Timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to trigger self-evolving plugin generation");
            _conversationalLogger.LogError($"Self-evolving plugin generation failed: {ex.Message}");
            
            return new PluginGenerationResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Execute GitHub operations through MCP
    /// </summary>
    public async Task<GitHubOperationResult> ExecuteGitHubOperationAsync(string operation, Dictionary<string, object> parameters)
    {
        try
        {
            if (!_mcpServers.ContainsKey("github"))
            {
                throw new InvalidOperationException("GitHub MCP server not connected. Call DiscoverGitHubMCPServerAsync first.");
            }

            var server = _mcpServers["github"];
            
            _conversationalLogger.LogAgentMessage("GitHubMCP", "github", $"Executing GitHub operation: {operation}", 
                $"Parameters: {JsonSerializer.Serialize(parameters)}");

            var mcpRequest = new MCPRequest
            {
                Method = "tools/call",
                Params = new Dictionary<string, object>
                {
                    ["name"] = operation,
                    ["arguments"] = parameters
                }
            };

            var response = await SendMCPRequestAsync(server, mcpRequest);
            
            if (response.Success)
            {
                _conversationalLogger.LogAgentMessage("GitHubMCP", "github", $"GitHub operation completed: {operation}", 
                    $"Result: {JsonSerializer.Serialize(response.Result)}");
                
                return new GitHubOperationResult
                {
                    Success = true,
                    Operation = operation,
                    Result = response.Result,
                    Timestamp = DateTime.UtcNow
                };
            }
            else
            {
                _conversationalLogger.LogError($"GitHub operation failed: {response.ErrorMessage}");
                
                return new GitHubOperationResult
                {
                    Success = false,
                    Operation = operation,
                    ErrorMessage = response.ErrorMessage,
                    Timestamp = DateTime.UtcNow
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute GitHub operation");
            _conversationalLogger.LogError($"GitHub operation failed: {ex.Message}");
            
            return new GitHubOperationResult
            {
                Success = false,
                Operation = operation,
                ErrorMessage = ex.Message,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Get repository information through GitHub MCP
    /// </summary>
    public async Task<GitHubRepositoryInfo> GetRepositoryInfoAsync(string owner, string repo)
    {
        var result = await ExecuteGitHubOperationAsync("get_repository", new Dictionary<string, object>
        {
            ["owner"] = owner,
            ["repo"] = repo
        });

        if (result.Success)
        {
            return new GitHubRepositoryInfo
            {
                Owner = owner,
                Repository = repo,
                Data = result.Result,
                Success = true,
                Timestamp = DateTime.UtcNow
            };
        }
        else
        {
            return new GitHubRepositoryInfo
            {
                Owner = owner,
                Repository = repo,
                Success = false,
                ErrorMessage = result.ErrorMessage,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// List issues through GitHub MCP
    /// </summary>
    public async Task<GitHubIssuesResult> ListIssuesAsync(string owner, string repo, string? state = "open")
    {
        var result = await ExecuteGitHubOperationAsync("list_issues", new Dictionary<string, object>
        {
            ["owner"] = owner,
            ["repo"] = repo,
            ["state"] = state ?? "open"
        });

        if (result.Success)
        {
            return new GitHubIssuesResult
            {
                Owner = owner,
                Repository = repo,
                Issues = result.Result,
                Success = true,
                Timestamp = DateTime.UtcNow
            };
        }
        else
        {
            return new GitHubIssuesResult
            {
                Owner = owner,
                Repository = repo,
                Success = false,
                ErrorMessage = result.ErrorMessage,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Create issue through GitHub MCP
    /// </summary>
    public async Task<GitHubIssueResult> CreateIssueAsync(string owner, string repo, string title, string body)
    {
        var result = await ExecuteGitHubOperationAsync("create_issue", new Dictionary<string, object>
        {
            ["owner"] = owner,
            ["repo"] = repo,
            ["title"] = title,
            ["body"] = body
        });

        if (result.Success)
        {
            return new GitHubIssueResult
            {
                Owner = owner,
                Repository = repo,
                Issue = result.Result,
                Success = true,
                Timestamp = DateTime.UtcNow
            };
        }
        else
        {
            return new GitHubIssueResult
            {
                Owner = owner,
                Repository = repo,
                Success = false,
                ErrorMessage = result.ErrorMessage,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Send MCP request to server
    /// </summary>
    private async Task<MCPResponse> SendMCPRequestAsync(MCPServer server, MCPRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            // Add authentication if available
            if (server.Authentication?.Token != null)
            {
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", server.Authentication.Token);
            }

            var response = await _httpClient.PostAsync(server.Endpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent);
                
                return new MCPResponse
                {
                    Success = true,
                    Result = result,
                    Timestamp = DateTime.UtcNow
                };
            }
            else
            {
                return new MCPResponse
                {
                    Success = false,
                    ErrorMessage = $"HTTP {response.StatusCode}: {responseContent}",
                    Timestamp = DateTime.UtcNow
                };
            }
        }
        catch (Exception ex)
        {
            return new MCPResponse
            {
                Success = false,
                ErrorMessage = ex.Message,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Parse MCP capabilities from discovery response
    /// </summary>
    private List<MCPCapability> ParseMCPCapabilities(Dictionary<string, object> schema)
    {
        var capabilities = new List<MCPCapability>();

        try
        {
            if (schema.TryGetValue("capabilities", out var capsObj) && capsObj is Dictionary<string, object> caps)
            {
                if (caps.TryGetValue("tools", out var toolsObj) && toolsObj is Dictionary<string, object> tools)
                {
                    foreach (var tool in tools)
                    {
                        capabilities.Add(new MCPCapability
                        {
                            Name = tool.Key,
                            Type = "tool",
                            Schema = tool.Value,
                            Timestamp = DateTime.UtcNow
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse MCP capabilities");
        }

        return capabilities;
    }

    /// <summary>
    /// Get connected MCP servers
    /// </summary>
    public Dictionary<string, MCPServer> GetConnectedServers()
    {
        return _mcpServers;
    }

    /// <summary>
    /// Disconnect from MCP server
    /// </summary>
    public async Task<bool> DisconnectServerAsync(string serverName)
    {
        try
        {
            if (_mcpServers.ContainsKey(serverName))
            {
                var server = _mcpServers[serverName];
                
                // Send shutdown request
                var shutdownRequest = new MCPRequest
                {
                    Method = "notifications/exit",
                    Params = new Dictionary<string, object>()
                };

                await SendMCPRequestAsync(server, shutdownRequest);
                _mcpServers.Remove(serverName);
                
                _conversationalLogger.LogSystemMessage($"Disconnected from MCP server: {serverName}", Color.Yellow);
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to disconnect from MCP server");
            return false;
        }
    }
}

/// <summary>
/// Interface for MCP Service
/// </summary>
public interface IMCPService
{
    Task<MCPDiscoveryResult> DiscoverGitHubMCPServerAsync(string? githubToken = null);
    Task<MCPDiscoveryResult> DiscoverMCPServerCapabilitiesAsync(MCPServer server);
    Task<PluginGenerationResult> TriggerSelfEvolvingPluginGenerationAsync(MCPServer server, MCPDiscoveryResult discovery);
    Task<GitHubOperationResult> ExecuteGitHubOperationAsync(string operation, Dictionary<string, object> parameters);
    Task<GitHubRepositoryInfo> GetRepositoryInfoAsync(string owner, string repo);
    Task<GitHubIssuesResult> ListIssuesAsync(string owner, string repo, string? state = "open");
    Task<GitHubIssueResult> CreateIssueAsync(string owner, string repo, string title, string body);
    Dictionary<string, MCPServer> GetConnectedServers();
    Task<bool> DisconnectServerAsync(string serverName);
} 