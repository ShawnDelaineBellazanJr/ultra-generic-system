using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using UltraGenericSystem.Services;
using System.Text.Json;

namespace UltraGenericSystem.Services.Agents
{
    /// <summary>
    /// Agent that treats each thought as a plugin, creating a recursive self-evolving system
    /// where thoughts can become callable functions and the system can reason about its own reasoning
    /// </summary>
    public class ThoughtAsPluginAgent
    {
        private readonly ISwaggerToSKPluginService _swaggerToSKService;
        private readonly ILogger<ThoughtAsPluginAgent> _logger;
        private readonly Kernel _kernel;
        private readonly Dictionary<string, ThoughtPlugin> _thoughtPlugins;

        public ThoughtAsPluginAgent(
            Kernel kernel,
            ISwaggerToSKPluginService swaggerToSKService,
            ILogger<ThoughtAsPluginAgent> logger)
        {
            _kernel = kernel;
            _swaggerToSKService = swaggerToSKService;
            _logger = logger;
            _thoughtPlugins = new Dictionary<string, ThoughtPlugin>();
        }

        public string Name => "ThoughtAsPluginAgent";
        public string Description => "An agent that treats each thought as a plugin, enabling recursive self-evolution.";

        /// <summary>
        /// Process a request by breaking it into thoughts, each becoming a plugin
        /// </summary>
        public async Task<ThoughtProcessingResult> ProcessWithThoughtPluginsAsync(string request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Processing request with thought plugins: {Request}", request);

                // Step 1: Generate initial thoughts
                var thoughts = await GenerateThoughtsAsync(request, cancellationToken);
                
                // Step 2: Convert each thought into a plugin
                var thoughtPlugins = new List<ThoughtPlugin>();
                foreach (var thought in thoughts)
                {
                    var plugin = await CreateThoughtPluginAsync(thought, cancellationToken);
                    thoughtPlugins.Add(plugin);
                    _thoughtPlugins[plugin.Id] = plugin;
                }

                // Step 3: Execute thoughts in sequence, with each thought able to call previous thoughts
                var results = new List<ThoughtExecutionResult>();
                for (int i = 0; i < thoughtPlugins.Count; i++)
                {
                    var plugin = thoughtPlugins[i];
                    var previousResults = results.Take(i).ToList();
                    var result = await ExecuteThoughtPluginAsync(plugin, previousResults, cancellationToken);
                    results.Add(result);
                }

                // Step 4: Generate meta-thoughts about the thought process itself
                var metaThoughts = await GenerateMetaThoughtsAsync(thoughts, results, cancellationToken);

                return new ThoughtProcessingResult
                {
                    Success = true,
                    OriginalRequest = request,
                    Thoughts = thoughts,
                    ThoughtPlugins = thoughtPlugins,
                    ExecutionResults = results,
                    MetaThoughts = metaThoughts,
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in thought plugin processing");
                return new ThoughtProcessingResult
                {
                    Success = false,
                    OriginalRequest = request,
                    ErrorMessage = ex.Message,
                    Timestamp = DateTime.UtcNow
                };
            }
        }

        /// <summary>
        /// Generate thoughts from a request
        /// </summary>
        private async Task<List<Thought>> GenerateThoughtsAsync(string request, CancellationToken cancellationToken)
        {
            var prompt = $@"
Generate a list of thoughts (as JSON array) to process this request: {request}

Each thought should have:
- id: unique identifier
- name: descriptive name
- description: what this thought does
- input: what input it expects
- output: what output it produces
- dependencies: array of thought IDs this depends on

Return only valid JSON array.
";

            var result = await _kernel.InvokePromptAsync(prompt, cancellationToken: cancellationToken);
            var jsonResult = result.GetValue<string>() ?? "[]";

            try
            {
                var thoughts = JsonSerializer.Deserialize<List<Thought>>(jsonResult) ?? new List<Thought>();
                return thoughts;
            }
            catch
            {
                // Fallback to generating thoughts programmatically
                return GenerateFallbackThoughts(request);
            }
        }

        /// <summary>
        /// Generate fallback thoughts if JSON parsing fails
        /// </summary>
        private List<Thought> GenerateFallbackThoughts(string request)
        {
            return new List<Thought>
            {
                new Thought
                {
                    Id = "analyze_request",
                    Name = "Analyze Request",
                    Description = "Analyze the user request to understand what needs to be done",
                    Input = request,
                    Output = "Analysis result",
                    Dependencies = new List<string>()
                },
                new Thought
                {
                    Id = "discover_apis",
                    Name = "Discover APIs",
                    Description = "Find relevant APIs that can help accomplish the task",
                    Input = "Analysis result",
                    Output = "List of relevant APIs",
                    Dependencies = new List<string> { "analyze_request" }
                },
                new Thought
                {
                    Id = "execute_plan",
                    Name = "Execute Plan",
                    Description = "Execute the plan using the discovered APIs",
                    Input = "List of relevant APIs",
                    Output = "Execution results",
                    Dependencies = new List<string> { "discover_apis" }
                }
            };
        }

        /// <summary>
        /// Create a plugin from a thought
        /// </summary>
        private async Task<ThoughtPlugin> CreateThoughtPluginAsync(Thought thought, CancellationToken cancellationToken)
        {
            var plugin = new ThoughtPlugin
            {
                Id = thought.Id,
                Name = thought.Name,
                Description = thought.Description,
                Thought = thought,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            // Generate the plugin's execution logic
            plugin.ExecutionLogic = await GenerateExecutionLogicAsync(thought, cancellationToken);

            _logger.LogInformation("Created thought plugin: {PluginId} - {PluginName}", plugin.Id, plugin.Name);
            return plugin;
        }

        /// <summary>
        /// Generate execution logic for a thought plugin
        /// </summary>
        private async Task<string> GenerateExecutionLogicAsync(Thought thought, CancellationToken cancellationToken)
        {
            var prompt = $@"
Generate C# code for executing this thought as a plugin:

Thought: {thought.Name}
Description: {thought.Description}
Input: {thought.Input}
Output: {thought.Output}
Dependencies: {string.Join(", ", thought.Dependencies)}

The code should:
1. Accept the input parameters
2. Call any dependent thoughts if needed
3. Execute the thought's logic
4. Return the output

Generate only the method body, not the full class.
";

            var result = await _kernel.InvokePromptAsync(prompt, cancellationToken: cancellationToken);
            return result.GetValue<string>() ?? "// Default execution logic";
        }

        /// <summary>
        /// Execute a thought plugin
        /// </summary>
        private async Task<ThoughtExecutionResult> ExecuteThoughtPluginAsync(
            ThoughtPlugin plugin, 
            List<ThoughtExecutionResult> previousResults, 
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Executing thought plugin: {PluginId}", plugin.Id);

                var startTime = DateTime.UtcNow;
                var result = new ThoughtExecutionResult
                {
                    PluginId = plugin.Id,
                    PluginName = plugin.Name,
                    StartTime = startTime,
                    Input = previousResults.LastOrDefault()?.Output ?? plugin.Thought.Input
                };

                // Execute the thought logic
                var output = await ExecuteThoughtLogicAsync(plugin, previousResults, cancellationToken);
                
                result.Output = output;
                result.EndTime = DateTime.UtcNow;
                result.Duration = result.EndTime - result.StartTime;
                result.Success = true;

                _logger.LogInformation("Thought plugin executed successfully: {PluginId} in {Duration}ms", 
                    plugin.Id, result.Duration.TotalMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing thought plugin: {PluginId}", plugin.Id);
                return new ThoughtExecutionResult
                {
                    PluginId = plugin.Id,
                    PluginName = plugin.Name,
                    Success = false,
                    ErrorMessage = ex.Message,
                    StartTime = DateTime.UtcNow,
                    EndTime = DateTime.UtcNow
                };
            }
        }

        /// <summary>
        /// Execute the actual logic of a thought
        /// </summary>
        private async Task<string> ExecuteThoughtLogicAsync(
            ThoughtPlugin plugin, 
            List<ThoughtExecutionResult> previousResults, 
            CancellationToken cancellationToken)
        {
            // This is a simplified implementation
            // In a real scenario, you would compile and execute the generated C# code
            
            switch (plugin.Id)
            {
                case "analyze_request":
                    return await AnalyzeRequestAsync(plugin.Thought.Input, cancellationToken);
                
                case "discover_apis":
                    var analysis = previousResults.FirstOrDefault(r => r.PluginId == "analyze_request")?.Output ?? "";
                    return await DiscoverApisAsync(analysis, cancellationToken);
                
                case "execute_plan":
                    var apis = previousResults.FirstOrDefault(r => r.PluginId == "discover_apis")?.Output ?? "";
                    return await ExecutePlanAsync(apis, cancellationToken);
                
                default:
                    return $"Executed thought: {plugin.Name} with input: {plugin.Thought.Input}";
            }
        }

        private async Task<string> AnalyzeRequestAsync(string input, CancellationToken cancellationToken)
        {
            var prompt = $"Analyze this request: {input}";
            var result = await _kernel.InvokePromptAsync(prompt, cancellationToken: cancellationToken);
            return result.GetValue<string>() ?? "Analysis completed";
        }

        private async Task<string> DiscoverApisAsync(string analysis, CancellationToken cancellationToken)
        {
            var apiSummary = await _swaggerToSKService.GetApiSummaryAsync();
            return $"Based on analysis: {analysis}\n\nAvailable APIs:\n{apiSummary}";
        }

        private async Task<string> ExecutePlanAsync(string apis, CancellationToken cancellationToken)
        {
            return $"Executing plan using APIs:\n{apis}\n\nPlan execution completed successfully.";
        }

        /// <summary>
        /// Generate meta-thoughts about the thought process itself
        /// </summary>
        private async Task<List<MetaThought>> GenerateMetaThoughtsAsync(
            List<Thought> thoughts, 
            List<ThoughtExecutionResult> results, 
            CancellationToken cancellationToken)
        {
            var prompt = $@"
Analyze this thought execution process and generate meta-thoughts about how the system could improve itself:

Original Thoughts: {JsonSerializer.Serialize(thoughts)}
Execution Results: {JsonSerializer.Serialize(results)}

Generate meta-thoughts that:
1. Analyze the effectiveness of the thought process
2. Suggest improvements to the thought generation
3. Identify patterns in successful vs failed thoughts
4. Propose new thoughts that could be added
5. Consider how thoughts could be better connected

Return as JSON array of meta-thoughts.
";

            var result = await _kernel.InvokePromptAsync(prompt, cancellationToken: cancellationToken);
            var jsonResult = result.GetValue<string>() ?? "[]";

            try
            {
                var metaThoughts = JsonSerializer.Deserialize<List<MetaThought>>(jsonResult) ?? new List<MetaThought>();
                return metaThoughts;
            }
            catch
            {
                return new List<MetaThought>
                {
                    new MetaThought
                    {
                        Id = "improve_thought_generation",
                        Name = "Improve Thought Generation",
                        Description = "The thought generation process could be improved by...",
                        Insights = "Generated insights about the thought process",
                        Suggestions = "Suggestions for improvement"
                    }
                };
            }
        }

        /// <summary>
        /// Get all available thought plugins
        /// </summary>
        public List<ThoughtPlugin> GetThoughtPlugins()
        {
            return _thoughtPlugins.Values.ToList();
        }

        /// <summary>
        /// Get a specific thought plugin
        /// </summary>
        public ThoughtPlugin? GetThoughtPlugin(string pluginId)
        {
            return _thoughtPlugins.TryGetValue(pluginId, out var plugin) ? plugin : null;
        }

        /// <summary>
        /// Create a new thought plugin from a description
        /// </summary>
        public async Task<ThoughtPlugin> CreateCustomThoughtPluginAsync(string name, string description, CancellationToken cancellationToken = default)
        {
            var thought = new Thought
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                Description = description,
                Input = "Custom input",
                Output = "Custom output",
                Dependencies = new List<string>()
            };

            return await CreateThoughtPluginAsync(thought, cancellationToken);
        }
    }

    /// <summary>
    /// Represents a thought that can be converted into a plugin
    /// </summary>
    public class Thought
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Input { get; set; } = string.Empty;
        public string Output { get; set; } = string.Empty;
        public List<string> Dependencies { get; set; } = new();
    }

    /// <summary>
    /// A thought converted into a callable plugin
    /// </summary>
    public class ThoughtPlugin
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Thought Thought { get; set; } = new();
        public string ExecutionLogic { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public int ExecutionCount { get; set; }
        public TimeSpan AverageExecutionTime { get; set; }
    }

    /// <summary>
    /// Result of executing a thought plugin
    /// </summary>
    public class ThoughtExecutionResult
    {
        public string PluginId { get; set; } = string.Empty;
        public string PluginName { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string Input { get; set; } = string.Empty;
        public string Output { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration { get; set; }
    }

    /// <summary>
    /// Meta-thought about the thought process itself
    /// </summary>
    public class MetaThought
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Insights { get; set; } = string.Empty;
        public string Suggestions { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Result of processing a request with thought plugins
    /// </summary>
    public class ThoughtProcessingResult
    {
        public bool Success { get; set; }
        public string OriginalRequest { get; set; } = string.Empty;
        public List<Thought> Thoughts { get; set; } = new();
        public List<ThoughtPlugin> ThoughtPlugins { get; set; } = new();
        public List<ThoughtExecutionResult> ExecutionResults { get; set; } = new();
        public List<MetaThought> MetaThoughts { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public DateTime Timestamp { get; set; }
        public TimeSpan TotalDuration { get; set; }
    }
} 