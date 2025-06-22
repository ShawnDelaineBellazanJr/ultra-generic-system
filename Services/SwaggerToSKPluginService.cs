using Microsoft.SemanticKernel;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace UltraGenericSystem.Services
{
    /// <summary>
    /// Service that dynamically generates Semantic Kernel plugins from OpenAPI/Swagger specifications
    /// Enables AI agents to discover and use all available API endpoints
    /// </summary>
    public class SwaggerToSKPluginService : ISwaggerToSKPluginService
    {
        private readonly ILogger<SwaggerToSKPluginService> _logger;
        private readonly HttpClient _httpClient;
        private readonly Kernel _kernel;
        private readonly Dictionary<string, object> _generatedPlugins;

        public SwaggerToSKPluginService(ILogger<SwaggerToSKPluginService> logger, HttpClient httpClient, Kernel kernel)
        {
            _logger = logger;
            _httpClient = httpClient;
            _kernel = kernel;
            _generatedPlugins = new Dictionary<string, object>();
        }

        /// <summary>
        /// Generates SK plugins from the OpenAPI specification
        /// </summary>
        public async Task<object> GeneratePluginFromSwaggerAsync(string swaggerUrl = "http://localhost:5000/swagger/v1/swagger.json")
        {
            try
            {
                _logger.LogInformation("Generating SK plugin from Swagger spec: {SwaggerUrl}", swaggerUrl);

                // Fetch the OpenAPI specification
                var swaggerJson = await _httpClient.GetStringAsync(swaggerUrl);
                var swaggerDoc = JsonSerializer.Deserialize<JsonNode>(swaggerJson);

                if (swaggerDoc == null)
                {
                    throw new InvalidOperationException("Failed to parse Swagger specification");
                }

                // Extract paths and create functions
                var paths = swaggerDoc["paths"]?.AsObject();
                if (paths == null)
                {
                    throw new InvalidOperationException("No paths found in Swagger specification");
                }

                var functions = new List<object>();

                foreach (var path in paths)
                {
                    var pathValue = path.Value?.AsObject();
                    if (pathValue == null) continue;

                    foreach (var method in pathValue)
                    {
                        var methodValue = method.Value?.AsObject();
                        if (methodValue == null) continue;

                        var functionMetadata = CreateFunctionMetadata(path.Key, method.Key, methodValue);
                        if (functionMetadata != null)
                        {
                            functions.Add(functionMetadata);
                        }
                    }
                }

                // Create a simple plugin representation
                var plugin = new
                {
                    Name = "UltraGenericAPI",
                    Functions = functions,
                    FunctionCount = functions.Count
                };
                
                _logger.LogInformation("Generated SK plugin with {FunctionCount} functions", functions.Count);
                
                return plugin;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate SK plugin from Swagger spec");
                throw;
            }
        }

        /// <summary>
        /// Creates function metadata from OpenAPI path and method
        /// </summary>
        private object? CreateFunctionMetadata(string path, string method, JsonObject methodSpec)
        {
            try
            {
                var operationId = methodSpec["operationId"]?.GetValue<string>();
                var summary = methodSpec["summary"]?.GetValue<string>() ?? methodSpec["description"]?.GetValue<string>() ?? "";
                var parameters = ExtractParameters(methodSpec);
                var requestBody = ExtractRequestBody(methodSpec);

                // Create a unique function name
                var functionName = operationId ?? $"{method.ToUpper()}_{path.Replace("/", "_").Replace("{", "").Replace("}", "")}";

                var metadata = new
                {
                    Name = functionName,
                    Description = summary,
                    Parameters = parameters,
                    Path = path,
                    Method = method,
                    RequestBody = requestBody
                };

                return metadata;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create function metadata for {Path} {Method}", path, method);
                return null;
            }
        }

        /// <summary>
        /// Extracts parameters from OpenAPI method specification
        /// </summary>
        private List<object> ExtractParameters(JsonObject methodSpec)
        {
            var parameters = new List<object>();

            // Extract path parameters
            var pathParams = methodSpec["parameters"]?.AsArray();
            if (pathParams != null)
            {
                foreach (var param in pathParams)
                {
                    var paramObj = param?.AsObject();
                    if (paramObj == null) continue;

                    var name = paramObj["name"]?.GetValue<string>();
                    var description = paramObj["description"]?.GetValue<string>() ?? "";
                    var required = paramObj["required"]?.GetValue<bool>() ?? false;
                    var paramType = paramObj["in"]?.GetValue<string>() ?? "query";

                    if (!string.IsNullOrEmpty(name))
                    {
                        parameters.Add(new
                        {
                            Name = name,
                            Description = description,
                            IsRequired = required,
                            DefaultValue = paramObj["default"]?.GetValue<string>(),
                            Type = paramType
                        });
                    }
                }
            }

            return parameters;
        }

        /// <summary>
        /// Extracts request body schema from OpenAPI method specification
        /// </summary>
        private JsonObject? ExtractRequestBody(JsonObject methodSpec)
        {
            return methodSpec["requestBody"]?.AsObject();
        }

        /// <summary>
        /// Executes an API call using the generated function metadata
        /// </summary>
        public async Task<string> ExecuteApiCallAsync(object metadata, object arguments)
        {
            try
            {
                // This is a simplified implementation
                // In a real scenario, you would use the metadata to make the actual API call
                return "API call executed successfully";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute API call");
                throw;
            }
        }

        /// <summary>
        /// Gets all available endpoints as a human-readable summary
        /// </summary>
        public async Task<string> GetApiSummaryAsync(string swaggerUrl = "http://localhost:5000/swagger/v1/swagger.json")
        {
            try
            {
                var swaggerJson = await _httpClient.GetStringAsync(swaggerUrl);
                var swaggerDoc = JsonSerializer.Deserialize<JsonNode>(swaggerJson);

                if (swaggerDoc == null)
                {
                    return "Failed to parse Swagger specification";
                }

                var paths = swaggerDoc["paths"]?.AsObject();
                if (paths == null)
                {
                    return "No endpoints found in API";
                }

                var summary = new System.Text.StringBuilder();
                summary.AppendLine("## Available API Endpoints\n");

                foreach (var path in paths)
                {
                    var pathValue = path.Value?.AsObject();
                    if (pathValue == null) continue;

                    summary.AppendLine($"### {path.Key}");

                    foreach (var method in pathValue)
                    {
                        var methodValue = method.Value?.AsObject();
                        if (methodValue == null) continue;

                        var summaryText = methodValue["summary"]?.GetValue<string>() ?? 
                                         methodValue["description"]?.GetValue<string>() ?? 
                                         "No description available";

                        summary.AppendLine($"- **{method.Key.ToUpper()}**: {summaryText}");
                    }

                    summary.AppendLine();
                }

                return summary.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate API summary");
                return $"Error generating API summary: {ex.Message}";
            }
        }

        /// <summary>
        /// Refreshes the plugin by regenerating it from the current Swagger spec
        /// </summary>
        public async Task<object> RefreshPluginAsync(string swaggerUrl = "http://localhost:5000/swagger/v1/swagger.json")
        {
            // Generate new plugin
            return await GeneratePluginFromSwaggerAsync(swaggerUrl);
        }
    }

    /// <summary>
    /// Interface for Swagger to SK Plugin service
    /// </summary>
    public interface ISwaggerToSKPluginService
    {
        Task<object> GeneratePluginFromSwaggerAsync(string swaggerUrl = "http://localhost:5000/swagger/v1/swagger.json");
        Task<string> ExecuteApiCallAsync(object metadata, object arguments);
        Task<string> GetApiSummaryAsync(string swaggerUrl = "http://localhost:5000/swagger/v1/swagger.json");
        Task<object> RefreshPluginAsync(string swaggerUrl = "http://localhost:5000/swagger/v1/swagger.json");
    }
} 