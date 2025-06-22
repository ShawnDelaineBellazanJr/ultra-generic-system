using Microsoft.AspNetCore.Mvc;
using UltraGenericSystem.Services;

namespace UltraGenericSystem.Controllers
{
    /// <summary>
    /// Controller for Swagger-to-SK Plugin functionality
    /// Enables AI agents to discover and use all available API endpoints
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SwaggerToSKController : ControllerBase
    {
        private readonly ISwaggerToSKPluginService _swaggerToSKService;
        private readonly ILogger<SwaggerToSKController> _logger;

        public SwaggerToSKController(ISwaggerToSKPluginService swaggerToSKService, ILogger<SwaggerToSKController> logger)
        {
            _swaggerToSKService = swaggerToSKService;
            _logger = logger;
        }

        /// <summary>
        /// Generates a Semantic Kernel plugin from the current OpenAPI specification
        /// </summary>
        [HttpPost("generate-plugin")]
        public async Task<IActionResult> GeneratePlugin([FromBody] GeneratePluginRequest request)
        {
            try
            {
                _logger.LogInformation("Generating SK plugin from Swagger spec");

                var plugin = await _swaggerToSKService.GeneratePluginFromSwaggerAsync(request.SwaggerUrl);

                // Extract properties from the plugin object using reflection
                var pluginType = plugin.GetType();
                var nameProperty = pluginType.GetProperty("Name");
                var functionsProperty = pluginType.GetProperty("Functions");
                var functionCountProperty = pluginType.GetProperty("FunctionCount");

                var pluginName = nameProperty?.GetValue(plugin)?.ToString() ?? "Unknown";
                var functions = functionsProperty?.GetValue(plugin) as System.Collections.IEnumerable;
                var functionCount = functionCountProperty?.GetValue(plugin) as int? ?? 0;

                return Ok(new GeneratePluginResponse
                {
                    Success = true,
                    PluginName = pluginName,
                    FunctionCount = functionCount,
                    Message = $"Generated plugin '{pluginName}' with {functionCount} functions"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate SK plugin");
                return BadRequest(new GeneratePluginResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets a human-readable summary of all available API endpoints
        /// </summary>
        [HttpGet("api-summary")]
        public async Task<IActionResult> GetApiSummary([FromQuery] string swaggerUrl = "http://localhost:5000/swagger/v1/swagger.json")
        {
            try
            {
                _logger.LogInformation("Generating API summary");

                var summary = await _swaggerToSKService.GetApiSummaryAsync(swaggerUrl);

                return Ok(new ApiSummaryResponse
                {
                    Success = true,
                    Summary = summary
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate API summary");
                return BadRequest(new ApiSummaryResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                });
            }
        }

        /// <summary>
        /// Refreshes the SK plugin by regenerating it from the current Swagger spec
        /// </summary>
        [HttpPost("refresh-plugin")]
        public async Task<IActionResult> RefreshPlugin([FromBody] RefreshPluginRequest request)
        {
            try
            {
                _logger.LogInformation("Refreshing SK plugin");

                var plugin = await _swaggerToSKService.RefreshPluginAsync(request.SwaggerUrl);

                // Extract properties from the plugin object using reflection
                var pluginType = plugin.GetType();
                var nameProperty = pluginType.GetProperty("Name");
                var functionsProperty = pluginType.GetProperty("Functions");
                var functionCountProperty = pluginType.GetProperty("FunctionCount");

                var pluginName = nameProperty?.GetValue(plugin)?.ToString() ?? "Unknown";
                var functions = functionsProperty?.GetValue(plugin) as System.Collections.IEnumerable;
                var functionCount = functionCountProperty?.GetValue(plugin) as int? ?? 0;

                return Ok(new RefreshPluginResponse
                {
                    Success = true,
                    PluginName = pluginName,
                    FunctionCount = functionCount,
                    Message = $"Refreshed plugin '{pluginName}' with {functionCount} functions"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh SK plugin");
                return BadRequest(new RefreshPluginResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets the current Swagger specification as JSON
        /// </summary>
        [HttpGet("swagger-spec")]
        public async Task<IActionResult> GetSwaggerSpec([FromQuery] string swaggerUrl = "http://localhost:5000/swagger/v1/swagger.json")
        {
            try
            {
                using var httpClient = new HttpClient();
                var swaggerJson = await httpClient.GetStringAsync(swaggerUrl);
                
                return Content(swaggerJson, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch Swagger spec");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Health check for the Swagger-to-SK service
        /// </summary>
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new
            {
                Status = "Healthy",
                Service = "SwaggerToSK",
                Timestamp = DateTime.UtcNow,
                Features = new[]
                {
                    "Plugin Generation",
                    "API Summary",
                    "Plugin Refresh",
                    "Swagger Spec Access"
                }
            });
        }
    }

    /// <summary>
    /// Request model for generating a SK plugin
    /// </summary>
    public class GeneratePluginRequest
    {
        public string SwaggerUrl { get; set; } = "http://localhost:5000/swagger/v1/swagger.json";
    }

    /// <summary>
    /// Response model for plugin generation
    /// </summary>
    public class GeneratePluginResponse
    {
        public bool Success { get; set; }
        public string? PluginName { get; set; }
        public int FunctionCount { get; set; }
        public string? Message { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Response model for API summary
    /// </summary>
    public class ApiSummaryResponse
    {
        public bool Success { get; set; }
        public string? Summary { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Request model for refreshing a plugin
    /// </summary>
    public class RefreshPluginRequest
    {
        public string SwaggerUrl { get; set; } = "http://localhost:5000/swagger/v1/swagger.json";
    }

    /// <summary>
    /// Response model for plugin refresh
    /// </summary>
    public class RefreshPluginResponse
    {
        public bool Success { get; set; }
        public string? PluginName { get; set; }
        public int FunctionCount { get; set; }
        public string? Message { get; set; }
        public string? ErrorMessage { get; set; }
    }
} 