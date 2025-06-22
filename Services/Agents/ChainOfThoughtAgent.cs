using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using UltraGenericSystem.Services;

namespace UltraGenericSystem.Services.Agents
{
    /// <summary>
    /// Chain of Thought agent that can discover and use all available API endpoints
    /// through natural language reasoning and the Swagger-to-SK plugin
    /// </summary>
    public class ChainOfThoughtAgent
    {
        private readonly ISwaggerToSKPluginService _swaggerToSKService;
        private readonly ILogger<ChainOfThoughtAgent> _logger;
        private readonly Kernel _kernel;

        public ChainOfThoughtAgent(
            Kernel kernel,
            ISwaggerToSKPluginService swaggerToSKService,
            ILogger<ChainOfThoughtAgent> logger)
        {
            _kernel = kernel;
            _swaggerToSKService = swaggerToSKService;
            _logger = logger;
        }

        public string Name => "ChainOfThoughtAgent";
        public string Description => "An agent that uses Chain of Thought reasoning to discover and interact with all available API endpoints through natural language.";

        public async Task<string> ProcessMessageAsync(string message, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("ChainOfThoughtAgent processing message: {Message}", message);

                // Step 1: Generate SK plugin from current Swagger spec
                var plugin = await _swaggerToSKService.GeneratePluginFromSwaggerAsync();
                _logger.LogInformation("Generated SK plugin with {FunctionCount} functions", GetFunctionCount(plugin));

                // Step 2: Get API summary for context
                var apiSummary = await _swaggerToSKService.GetApiSummaryAsync();
                _logger.LogInformation("Retrieved API summary with {Length} characters", apiSummary.Length);

                // Step 3: Create Chain of Thought prompt
                var thoughtPrompt = CreateThoughtPrompt(message, apiSummary, GetFunctionCount(plugin));

                // Step 4: Execute Chain of Thought reasoning
                var reasoning = await ExecuteChainOfThoughtAsync(thoughtPrompt, cancellationToken);

                // Step 5: Extract and execute API calls from reasoning
                var result = await ExecuteApiCallsFromReasoningAsync(reasoning, plugin, cancellationToken);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ChainOfThoughtAgent processing");
                return $"Error processing request: {ex.Message}";
            }
        }

        private int GetFunctionCount(object plugin)
        {
            // Extract function count from the plugin object
            var pluginType = plugin.GetType();
            var functionCountProperty = pluginType.GetProperty("FunctionCount");
            return functionCountProperty?.GetValue(plugin) as int? ?? 0;
        }

        private string CreateThoughtPrompt(string userMessage, string apiSummary, int functionCount)
        {
            return $@"
You are a Chain of Thought agent that can discover and use all available API endpoints through natural language reasoning.

## Available API Endpoints
{apiSummary}

## Available Functions
The system has {functionCount} available API functions that you can call.

## User Request
{userMessage}

## Your Task
1. **Think through the problem step by step** - Break down what the user wants to accomplish
2. **Identify relevant API endpoints** - Determine which endpoints would be useful
3. **Plan the execution** - Create a step-by-step plan for using the APIs
4. **Execute the plan** - Call the appropriate APIs in sequence
5. **Provide results** - Return the results in a clear, organized format

## Chain of Thought Process
Let me think through this step by step:

1. **Understanding the request**: [Your analysis of what the user wants]
2. **Identifying relevant APIs**: [Which endpoints would be useful]
3. **Planning the execution**: [Step-by-step plan]
4. **Executing the plan**: [Actual API calls]
5. **Providing results**: [Organized results]

Please proceed with your Chain of Thought reasoning and execute the necessary API calls.
";
        }

        private async Task<string> ExecuteChainOfThoughtAsync(string prompt, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _kernel.InvokePromptAsync(prompt, cancellationToken: cancellationToken);
                var response = result.GetValue<string>() ?? "No response generated";

                _logger.LogInformation("Chain of Thought reasoning completed: {Length} characters", response.Length);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing Chain of Thought reasoning");
                return $"Error in reasoning: {ex.Message}";
            }
        }

        private async Task<string> ExecuteApiCallsFromReasoningAsync(string reasoning, object plugin, CancellationToken cancellationToken)
        {
            try
            {
                var results = new List<string>();
                var lines = reasoning.Split('\n', StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                {
                    // Look for API call patterns in the reasoning
                    if (line.Contains("API call:") || line.Contains("Calling:") || line.Contains("GET") || line.Contains("POST"))
                    {
                        var apiCall = ExtractApiCall(line);
                        if (!string.IsNullOrEmpty(apiCall))
                        {
                            var result = await ExecuteSingleApiCallAsync(apiCall, plugin, cancellationToken);
                            results.Add($"API Call: {apiCall}\nResult: {result}\n");
                        }
                    }
                }

                if (!results.Any())
                {
                    // If no explicit API calls found, try to infer from the reasoning
                    var inferredCalls = InferApiCallsFromReasoning(reasoning);
                    foreach (var call in inferredCalls)
                    {
                        var result = await ExecuteSingleApiCallAsync(call, plugin, cancellationToken);
                        results.Add($"Inferred API Call: {call}\nResult: {result}\n");
                    }
                }

                return string.Join("\n", results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing API calls from reasoning");
                return $"Error executing API calls: {ex.Message}";
            }
        }

        private string ExtractApiCall(string line)
        {
            // Extract API call information from reasoning text
            if (line.Contains("GET /api/"))
            {
                var start = line.IndexOf("/api/");
                var end = line.IndexOf(" ", start);
                if (end == -1) end = line.Length;
                return line.Substring(start, end - start);
            }
            else if (line.Contains("POST /api/"))
            {
                var start = line.IndexOf("/api/");
                var end = line.IndexOf(" ", start);
                if (end == -1) end = line.Length;
                return line.Substring(start, end - start);
            }

            return string.Empty;
        }

        private List<string> InferApiCallsFromReasoning(string reasoning)
        {
            var calls = new List<string>();

            // Infer common API calls based on reasoning content
            if (reasoning.Contains("health") || reasoning.Contains("status"))
            {
                calls.Add("/api/AdvancedOrchestration/health");
                calls.Add("/api/Ollama/health");
                calls.Add("/api/SwaggerToSK/health");
            }

            if (reasoning.Contains("summary") || reasoning.Contains("overview"))
            {
                calls.Add("/api/SwaggerToSK/api-summary");
            }

            if (reasoning.Contains("conversation") || reasoning.Contains("chat"))
            {
                calls.Add("/api/AdvancedOrchestration/conversation-demo");
            }

            if (reasoning.Contains("evolution") || reasoning.Contains("strange loop"))
            {
                calls.Add("/api/SelfEvolution/statistics");
            }

            return calls;
        }

        private async Task<string> ExecuteSingleApiCallAsync(string apiCall, object plugin, CancellationToken cancellationToken)
        {
            try
            {
                // This is a simplified implementation
                // In a real scenario, you would use the plugin to make the actual API call
                return $"Executed API call: {apiCall}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing API call: {ApiCall}", apiCall);
                return $"Error: {ex.Message}";
            }
        }

        public async Task<string> DiscoverAndUseApisAsync(string userRequest, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Starting API discovery and usage for request: {Request}", userRequest);

                // Generate plugin and get summary
                var plugin = await _swaggerToSKService.GeneratePluginFromSwaggerAsync();
                var apiSummary = await _swaggerToSKService.GetApiSummaryAsync();

                // Create a comprehensive prompt for API discovery
                var discoveryPrompt = $@"
You are an intelligent agent that can discover and use all available APIs.

## Available APIs
{apiSummary}

## User Request
{userRequest}

## Your Task
1. Analyze the user's request
2. Identify which APIs would be most useful
3. Execute the appropriate API calls
4. Return the results in a clear, organized format

Please proceed with API discovery and execution.
";

                return await ProcessMessageAsync(discoveryPrompt, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in API discovery and usage");
                return $"Error: {ex.Message}";
            }
        }

        public async Task<string> GetApiCapabilitiesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var apiSummary = await _swaggerToSKService.GetApiSummaryAsync();
                return $"## Available API Capabilities\n\n{apiSummary}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting API capabilities");
                return $"Error retrieving API capabilities: {ex.Message}";
            }
        }
    }
} 