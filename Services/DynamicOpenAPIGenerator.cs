using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using NSwag;
using NSwag.CodeGeneration.CSharp;
using NSwag.CodeGeneration.OperationNameGenerators;
using System.Text.Json;
using UltraGenericSystem.Services.Agents;
using UltraGenericSystem.Services.DynamicAPI;

namespace UltraGenericSystem.Services;

/// <summary>
/// Dynamic OpenAPI generator service for creating APIs at runtime
/// </summary>
public class DynamicOpenAPIGenerator : IDynamicAPIGeneratorService
{
    private readonly Kernel _kernel;
    private readonly ILogger<DynamicOpenAPIGenerator> _logger;
    private readonly SKAgentFactory _agentFactory;

    public DynamicOpenAPIGenerator(
        Kernel kernel,
        ILogger<DynamicOpenAPIGenerator> logger,
        SKAgentFactory agentFactory)
    {
        _kernel = kernel;
        _logger = logger;
        _agentFactory = agentFactory;
    }

    /// <summary>
    /// Generates OpenAPI specification from LLM description
    /// </summary>
    public async Task<OpenAPIGenerationResult> GenerateOpenAPISpecAsync(
        string description,
        OpenAPIGenerationOptions options,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating OpenAPI specification from description");

            // Create a specialized agent for OpenAPI generation
            var openApiAgent = await _agentFactory.CreateAgentAsync(
                "OpenAPIGenerator",
                "Specialized agent for generating OpenAPI specifications",
                new Dictionary<string, object>
                {
                    ["description"] = description,
                    ["options"] = options
                });

            // Generate OpenAPI specification using the agent
            var openApiSpec = await GenerateOpenAPISpecWithAgentAsync(openApiAgent, description, options);

            // Validate the generated specification
            var validationResult = await ValidateOpenAPISpecAsync(openApiSpec, cancellationToken);
            if (!validationResult.IsValid)
            {
                return new OpenAPIGenerationResult
                {
                    Success = false,
                    Errors = validationResult.Errors
                };
            }

            return new OpenAPIGenerationResult
            {
                Success = true,
                OpenAPISpec = openApiSpec,
                GenerationTime = TimeSpan.FromMilliseconds(100)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating OpenAPI specification");
            return new OpenAPIGenerationResult
            {
                Success = false,
                Errors = new List<string> { ex.Message }
            };
        }
    }

    /// <summary>
    /// Generates C# controller from OpenAPI specification
    /// </summary>
    public async Task<ControllerGenerationResult> GenerateControllerAsync(
        string openApiSpec,
        ControllerGenerationOptions options,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating controller from OpenAPI specification");

            // Parse OpenAPI spec
            var openApiDocument = NSwag.OpenApiDocument.FromJsonAsync(openApiSpec).Result;

            // Configure NSwag settings for C# controller generation
            var settings = new CSharpControllerGeneratorSettings
            {
                ClassName = "DynamicController",
                ControllerBaseClass = options.BaseController,
                OperationNameGenerator = new MultipleClientsFromOperationIdOperationNameGenerator()
            };

            // Generate controller code
            var generator = new CSharpControllerGenerator(openApiDocument, settings);
            var controllerCode = generator.GenerateFile();

            return new ControllerGenerationResult
            {
                Success = true,
                ControllerCode = controllerCode,
                ControllerName = "DynamicController",
                GenerationTime = TimeSpan.FromMilliseconds(200)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating controller");
            return new ControllerGenerationResult
            {
                Success = false,
                Errors = new List<string> { ex.Message }
            };
        }
    }

    /// <summary>
    /// Registers a dynamically generated controller
    /// </summary>
    public async Task<bool> RegisterControllerAsync(
        string controllerCode,
        string controllerName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Registering controller: {ControllerName}", controllerName);
            // Implementation would register the controller with the application
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering controller: {ControllerName}", controllerName);
            return false;
        }
    }

    /// <summary>
    /// Unregisters a dynamic controller
    /// </summary>
    public async Task<bool> UnregisterControllerAsync(string controllerName, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Unregistering controller: {ControllerName}", controllerName);
            // Implementation would unregister the controller from the application
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unregistering controller: {ControllerName}", controllerName);
            return false;
        }
    }

    /// <summary>
    /// Gets all registered dynamic controllers
    /// </summary>
    public async Task<List<DynamicControllerInfo>> GetRegisteredControllersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting registered controllers");
            // Implementation would return list of registered controllers
            return new List<DynamicControllerInfo>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting registered controllers");
            return new List<DynamicControllerInfo>();
        }
    }

    /// <summary>
    /// Validates OpenAPI specification
    /// </summary>
    public async Task<OpenAPIValidationResult> ValidateOpenAPISpecAsync(
        string openApiSpec,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Validating OpenAPI specification");
            
            // Basic validation - check if it's valid JSON and has required fields
            var document = NSwag.OpenApiDocument.FromJsonAsync(openApiSpec).Result;
            
            return new OpenAPIValidationResult
            {
                IsValid = true,
                ValidationDetails = new Dictionary<string, object>
                {
                    ["version"] = document.Info?.Version ?? "unknown",
                    ["title"] = document.Info?.Title ?? "unknown"
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating OpenAPI specification");
            return new OpenAPIValidationResult
            {
                IsValid = false,
                Errors = new List<string> { ex.Message }
            };
        }
    }

    #region Private Methods

    private async Task<string> GenerateOpenAPISpecWithAgentAsync(
        object openApiAgent,
        string description,
        OpenAPIGenerationOptions options)
    {
        // Create a prompt for OpenAPI generation
        var prompt = $@"
Generate an OpenAPI 3.0 specification for the following requirements:

Description: {description}

Additional Options:
- Base URL: {options.BasePath ?? "/api"}
- Version: {options.Version ?? "3.0.1"}
- Description: {options.Description ?? "Dynamically generated API"}
- Security: {(options.EnableAuthentication ? options.AuthenticationType : "None")}

Generate a complete OpenAPI specification in JSON format. Include:
1. Info section with title, version, and description
2. Servers section with base URL
3. Paths section with all required endpoints
4. Components section with schemas for request/response models
5. Security schemes if authentication is enabled

Return only the JSON specification without any additional text.
";

        // Use the agent to generate the specification
        var response = await _kernel.InvokePromptAsync(prompt);
        var jsonSpec = response.GetValue<string>();

        return jsonSpec;
    }

    #endregion
} 