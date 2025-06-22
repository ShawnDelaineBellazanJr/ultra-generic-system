using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using UltraGenericSystem.Models;
using UltraGenericSystem.Services;
using UltraGenericSystem.Services.Agents;
using Spectre.Console;

namespace UltraGenericSystem.Controllers;

/// <summary>
/// Advanced orchestration controller demonstrating SK Agent Framework features
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AdvancedOrchestrationController : ControllerBase
{
    private readonly IAgentOrchestrator _orchestrator;
    private readonly ILogger<AdvancedOrchestrationController> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly Kernel _kernel;
    private readonly ISwaggerToSKPluginService _swaggerToSKService;

    public AdvancedOrchestrationController(
        IAgentOrchestrator orchestrator,
        ILogger<AdvancedOrchestrationController> logger,
        ILoggerFactory loggerFactory,
        Kernel kernel,
        ISwaggerToSKPluginService swaggerToSKService)
    {
        _orchestrator = orchestrator;
        _logger = logger;
        _loggerFactory = loggerFactory;
        _kernel = kernel;
        _swaggerToSKService = swaggerToSKService;
    }

    /// <summary>
    /// Execute advanced orchestration with structured data
    /// </summary>
    [HttpPost("advanced")]
    public async Task<ActionResult<AdvancedAgentResponse<object>>> ExecuteAdvancedOrchestration(
        [FromBody] AdvancedAgentRequest<object, object> request)
    {
        try
        {
            _logger.LogInformation("Executing advanced orchestration for operation: {Operation}", request.Operation);
            
            var result = await _orchestrator.ExecuteAdvancedOrchestrationAsync(request);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Advanced orchestration failed");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Execute structured orchestration with custom transforms
    /// </summary>
    [HttpPost("structured")]
    public async Task<ActionResult<StructuredOutput<object>>> ExecuteStructuredOrchestration(
        [FromBody] StructuredInput<object> input,
        [FromQuery] bool enableStructuredData = true,
        [FromQuery] bool enableResponseCallbacks = true,
        [FromQuery] bool enableHumanInTheLoop = false,
        [FromQuery] int timeoutMinutes = 5)
    {
        try
        {
            var config = new AdvancedOrchestrationConfig
            {
                EnableStructuredData = enableStructuredData,
                EnableResponseCallbacks = enableResponseCallbacks,
                EnableHumanInTheLoop = enableHumanInTheLoop,
                Timeout = TimeSpan.FromMinutes(timeoutMinutes)
            };

            _logger.LogInformation("Executing structured orchestration");
            
            var result = await _orchestrator.ExecuteStructuredOrchestrationAsync<object, object>(input, config);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Structured orchestration failed");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Execute workflow with multiple orchestration patterns
    /// </summary>
    [HttpPost("workflow")]
    public async Task<ActionResult<WorkflowExecutionResult>> ExecuteWorkflow(
        [FromBody] object input,
        [FromQuery] string[] steps)
    {
        try
        {
            var config = new AdvancedOrchestrationConfig
            {
                EnableStructuredData = true,
                EnableResponseCallbacks = true,
                Timeout = TimeSpan.FromMinutes(10)
            };

            _logger.LogInformation("Executing workflow with {StepCount} steps", steps.Length);
            
            var result = await _orchestrator.ExecuteWorkflowAsync<object, object>(
                input, 
                steps.ToList(), 
                config);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Workflow execution failed");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Analyze entity with advanced orchestration
    /// </summary>
    [HttpPost("analyze")]
    public async Task<ActionResult<EntityAnalysisResult>> AnalyzeEntity(
        [FromBody] object entity,
        [FromQuery] string entityType = "generic",
        [FromQuery] bool enableResponseCallbacks = true,
        [FromQuery] int timeoutMinutes = 3)
    {
        try
        {
            var config = new AdvancedOrchestrationConfig
            {
                EnableResponseCallbacks = enableResponseCallbacks,
                Timeout = TimeSpan.FromMinutes(timeoutMinutes)
            };

            _logger.LogInformation("Analyzing entity of type: {EntityType}", entityType);
            
            var result = await _orchestrator.AnalyzeEntityAsync(entity, entityType, config);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Entity analysis failed");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Execute orchestration with human-in-the-loop
    /// </summary>
    [HttpPost("human-in-loop")]
    public async Task<ActionResult<AdvancedAgentResponse<object>>> ExecuteWithHumanInLoop(
        [FromBody] AdvancedAgentRequest<object, object> request)
    {
        try
        {
            // Configure for human-in-the-loop
            request.HumanInTheLoopConfig.EnableUserInput = true;
            request.HumanInTheLoopConfig.EnableApprovalWorkflow = true;
            request.HumanInTheLoopConfig.UserInputTimeout = TimeSpan.FromMinutes(2);
            request.HumanInTheLoopConfig.AllowedUserActions = new List<string> { "approve", "reject", "modify" };

            _logger.LogInformation("Executing orchestration with human-in-the-loop");
            
            var result = await _orchestrator.ExecuteAdvancedOrchestrationAsync(request);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Human-in-the-loop orchestration failed");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Execute orchestration with custom transforms
    /// </summary>
    [HttpPost("custom-transforms")]
    public async Task<ActionResult<AdvancedAgentResponse<object>>> ExecuteWithCustomTransforms(
        [FromBody] AdvancedAgentRequest<object, object> request,
        [FromQuery] string? inputTransformScript = null,
        [FromQuery] string? outputTransformScript = null)
    {
        try
        {
            // Configure custom transforms
            request.CustomTransformConfig.InputTransformScript = inputTransformScript;
            request.CustomTransformConfig.OutputTransformScript = outputTransformScript;
            request.CustomTransformConfig.EnableValidation = true;

            _logger.LogInformation("Executing orchestration with custom transforms");
            
            var result = await _orchestrator.ExecuteAdvancedOrchestrationAsync(request);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Custom transforms orchestration failed");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Execute orchestration with cancellation support
    /// </summary>
    [HttpPost("cancellable")]
    public async Task<ActionResult<AdvancedAgentResponse<object>>> ExecuteCancellable(
        [FromBody] AdvancedAgentRequest<object, object> request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Configure for cancellation
            request.OrchestrationConfig.EnableCancellation = true;
            request.CancellationToken = cancellationToken;

            _logger.LogInformation("Executing cancellable orchestration");
            
            var result = await _orchestrator.ExecuteAdvancedOrchestrationAsync(request);
            
            return Ok(result);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Orchestration was cancelled");
            return StatusCode(499, new { message = "Operation was cancelled" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cancellable orchestration failed");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get orchestration patterns and capabilities
    /// </summary>
    [HttpGet("patterns")]
    public ActionResult<object> GetOrchestrationPatterns()
    {
        return Ok(new
        {
            patterns = new[]
            {
                "sequential",
                "concurrent", 
                "groupchat",
                "handoff"
            },
            features = new[]
            {
                "structured_data",
                "response_callbacks",
                "human_in_the_loop",
                "custom_transforms",
                "cancellation",
                "timeouts",
                "workflows"
            },
            capabilities = new
            {
                supportsStructuredData = true,
                supportsResponseCallbacks = true,
                supportsHumanInTheLoop = true,
                supportsCustomTransforms = true,
                supportsCancellation = true,
                supportsTimeouts = true,
                supportsWorkflows = true
            }
        });
    }

    /// <summary>
    /// Demonstrates agent conversations in English using real AI agents
    /// </summary>
    [HttpPost("conversation-demo")]
    public async Task<IActionResult> StartConversationDemo([FromBody] ConversationDemoRequest request)
    {
        try
        {
            var conversationalLogger = HttpContext.RequestServices.GetRequiredService<IConversationalLogger>();
            var ollamaService = HttpContext.RequestServices.GetService<IOllamaService>();
            
            conversationalLogger.LogConversationStart("Real AI Agent Conversation", request.Task ?? "General discussion");
            
            if (ollamaService == null)
            {
                conversationalLogger.LogError("Ollama service not available. Please ensure Ollama is running and configured.");
                return BadRequest(new
                {
                    success = false,
                    error = "Ollama service not available. Please ensure Ollama is running and configured.",
                    timestamp = DateTime.UtcNow
                });
            }

            // Check if Ollama is available
            try
            {
                var healthCheck = await ollamaService.ListModelsAsync();
                conversationalLogger.LogAgentMessage("System", "system", $"Ollama connected successfully. Available models: {string.Join(", ", healthCheck)}");
            }
            catch (Exception ex)
            {
                conversationalLogger.LogError($"Failed to connect to Ollama: {ex.Message}");
                return BadRequest(new
                {
                    success = false,
                    error = $"Failed to connect to Ollama: {ex.Message}",
                    timestamp = DateTime.UtcNow
                });
            }

            // Create agent personas with specific instructions
            var orchestratorInstructions = "You are an AI orchestrator agent. Your role is to coordinate other agents, manage task flow, and make strategic decisions. Be concise, decisive, and focus on moving the team forward.";
            var plannerInstructions = "You are an AI planner agent. Your role is to analyze tasks, break them down into manageable steps, and create detailed execution plans. Be thorough and systematic in your approach.";
            var makerInstructions = "You are an AI maker agent. Your role is to execute implementations, write code, and build solutions. Be practical and focus on getting things done.";
            var checkerInstructions = "You are an AI checker agent. Your role is to review work, ensure quality standards, and identify potential issues. Be thorough and detail-oriented.";
            var reflectorInstructions = "You are an AI reflector agent. Your role is to analyze progress, suggest improvements, and learn from experiences. Be insightful and forward-thinking.";

            // Simulate a real conversation with AI agents
            var task = request.Task ?? "Design a microservices architecture";
            
            // Orchestrator starts the conversation
            var orchestratorPrompt = $"{orchestratorInstructions}\n\nTask: {task}\n\nStart the conversation by introducing the task and asking the team to collaborate.";
            var orchestratorRequest = new OllamaRequest
            {
                Model = ollamaService.Config.DefaultModel,
                Prompt = orchestratorPrompt,
                System = "You are a team orchestrator. Keep responses concise and focused on coordination.",
                Stream = false
            };
            
            var orchestratorResponse = await ollamaService.SendMessageAsync(orchestratorRequest);
            conversationalLogger.LogAgentMessage("OrchestratorAgent", "orchestrator", orchestratorResponse.Response);

            // Planner analyzes the task
            var plannerPrompt = $"{plannerInstructions}\n\nTask: {task}\n\nPrevious message: {orchestratorResponse.Response}\n\nAnalyze this task and create a plan.";
            var plannerRequest = new OllamaRequest
            {
                Model = ollamaService.Config.DefaultModel,
                Prompt = plannerPrompt,
                System = "You are a strategic planner. Focus on breaking down complex tasks into actionable steps.",
                Stream = false
            };
            
            var plannerResponse = await ollamaService.SendMessageAsync(plannerRequest);
            conversationalLogger.LogAgentMessage("PlannerAgent", "planner", plannerResponse.Response);

            // Maker responds to the plan
            var makerPrompt = $"{makerInstructions}\n\nTask: {task}\n\nPlanner's analysis: {plannerResponse.Response}\n\nRespond to the plan and indicate what you can implement.";
            var makerRequest = new OllamaRequest
            {
                Model = ollamaService.Config.DefaultModel,
                Prompt = makerPrompt,
                System = "You are a practical implementer. Focus on execution and technical details.",
                Stream = false
            };
            
            var makerResponse = await ollamaService.SendMessageAsync(makerRequest);
            conversationalLogger.LogAgentMessage("MakerAgent", "maker", makerResponse.Response);

            // Checker reviews the approach
            var checkerPrompt = $"{checkerInstructions}\n\nTask: {task}\n\nPlanner's plan: {plannerResponse.Response}\n\nMaker's response: {makerResponse.Response}\n\nReview the approach and identify potential issues.";
            var checkerRequest = new OllamaRequest
            {
                Model = ollamaService.Config.DefaultModel,
                Prompt = checkerPrompt,
                System = "You are a quality assurance specialist. Focus on identifying risks and ensuring standards.",
                Stream = false
            };
            
            var checkerResponse = await ollamaService.SendMessageAsync(checkerRequest);
            conversationalLogger.LogAgentMessage("CheckerAgent", "checker", checkerResponse.Response);

            // Reflector provides insights
            var reflectorPrompt = $"{reflectorInstructions}\n\nTask: {task}\n\nTeam discussion:\n- Planner: {plannerResponse.Response}\n- Maker: {makerResponse.Response}\n- Checker: {checkerResponse.Response}\n\nProvide insights and suggestions for improvement.";
            var reflectorRequest = new OllamaRequest
            {
                Model = ollamaService.Config.DefaultModel,
                Prompt = reflectorPrompt,
                System = "You are a strategic thinker. Focus on learning and improvement opportunities.",
                Stream = false
            };
            
            var reflectorResponse = await ollamaService.SendMessageAsync(reflectorRequest);
            conversationalLogger.LogAgentMessage("ReflectorAgent", "reflector", reflectorResponse.Response);

            // Orchestrator makes a decision
            var decisionPrompt = $"{orchestratorInstructions}\n\nTask: {task}\n\nTeam discussion:\n- Planner: {plannerResponse.Response}\n- Maker: {makerResponse.Response}\n- Checker: {checkerResponse.Response}\n- Reflector: {reflectorResponse.Response}\n\nMake a decision on next steps and provide reasoning.";
            var decisionRequest = new OllamaRequest
            {
                Model = ollamaService.Config.DefaultModel,
                Prompt = decisionPrompt,
                System = "You are a decisive leader. Make clear decisions and provide reasoning.",
                Stream = false
            };
            
            var decisionResponse = await ollamaService.SendMessageAsync(decisionRequest);
            conversationalLogger.LogAgentDecision("OrchestratorAgent", decisionResponse.Response, "Based on team input and analysis");

            conversationalLogger.LogConversationEnd("Real AI Agent Conversation", "Team successfully collaborated using real AI agents");
            
            return Ok(new
            {
                success = true,
                message = "Real AI agent conversation completed successfully",
                conversationHistory = conversationalLogger.GetConversationLog(),
                task = request.Task,
                model = ollamaService.Config.DefaultModel,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            var conversationalLogger = HttpContext.RequestServices.GetRequiredService<IConversationalLogger>();
            conversationalLogger.LogError($"Real AI conversation demo failed: {ex.Message}");
            
            return BadRequest(new
            {
                success = false,
                error = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Test the Chain of Thought agent that can discover and use all available APIs
    /// </summary>
    [HttpPost("chain-of-thought")]
    public async Task<IActionResult> TestChainOfThought([FromBody] ChainOfThoughtRequest request)
    {
        try
        {
            _logger.LogInformation("Testing Chain of Thought agent with request: {Request}", request.UserRequest);

            // Create Chain of Thought agent
            var chainOfThoughtAgent = new ChainOfThoughtAgent(
                _kernel,
                _swaggerToSKService,
                _loggerFactory.CreateLogger<ChainOfThoughtAgent>());

            // Execute the request
            var result = await chainOfThoughtAgent.DiscoverAndUseApisAsync(request.UserRequest);

            return Ok(new ChainOfThoughtResponse
            {
                Success = true,
                UserRequest = request.UserRequest,
                Result = result,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing Chain of Thought agent");
            return BadRequest(new ChainOfThoughtResponse
            {
                Success = false,
                UserRequest = request.UserRequest,
                ErrorMessage = ex.Message,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Get available API capabilities through Chain of Thought agent
    /// </summary>
    [HttpGet("api-capabilities")]
    public async Task<IActionResult> GetApiCapabilities()
    {
        try
        {
            _logger.LogInformation("Getting API capabilities through Chain of Thought agent");

            // Create Chain of Thought agent
            var chainOfThoughtAgent = new ChainOfThoughtAgent(
                _kernel,
                _swaggerToSKService,
                _loggerFactory.CreateLogger<ChainOfThoughtAgent>());

            // Get capabilities
            var capabilities = await chainOfThoughtAgent.GetApiCapabilitiesAsync();

            return Ok(new ApiCapabilitiesResponse
            {
                Success = true,
                Capabilities = capabilities,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting API capabilities");
            return BadRequest(new ApiCapabilitiesResponse
            {
                Success = false,
                ErrorMessage = ex.Message,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    // --- PRODUCTION-READY CONTINUOUS LOOP WORKFLOW BUILDER ---
    private class ContinuousLoopWorkflow
    {
        private readonly IOllamaService _ollamaService;
        private readonly IConversationalLogger _logger;
        private readonly string _model;
        private readonly string _task;
        private readonly int _maxIterations;
        private readonly double _convergenceThreshold;

        public List<IterationResult> Iterations { get; } = new();
        public double FinalConvergenceScore { get; private set; }
        public int TotalIterations => Iterations.Count;
        public bool Converged { get; private set; }
        public string ConversationLog => _logger.GetConversationLog();

        public ContinuousLoopWorkflow(IOllamaService ollamaService, IConversationalLogger logger, string model, string task, int maxIterations, double convergenceThreshold)
        {
            _ollamaService = ollamaService;
            _logger = logger;
            _model = model;
            _task = task;
            _maxIterations = maxIterations;
            _convergenceThreshold = convergenceThreshold;
        }

        public async Task RunAsync()
        {
            var currentIteration = 0;
            var convergenceScore = 0.0;
            var shouldContinue = true;

            while (shouldContinue && currentIteration < _maxIterations)
            {
                currentIteration++;
                _logger.LogSystemMessage($"🔄 Starting Iteration {currentIteration}/{_maxIterations}", Color.Blue);
                var iterationResult = new IterationResult
                {
                    IterationNumber = currentIteration,
                    StartTime = DateTime.UtcNow,
                    ConversationHistory = new List<string>()
                };
                try
                {
                    // Orchestrator
                    var orchestratorResponse = await RunAgentAsync("OrchestratorAgent", OrchestratorPrompt(currentIteration, convergenceScore), "You are a team orchestrator focused on continuous improvement and convergence.");
                    iterationResult.ConversationHistory.Add($"Orchestrator: {orchestratorResponse}");

                    // Planner
                    var plannerResponse = await RunAgentAsync("PlannerAgent", PlannerPrompt(currentIteration, orchestratorResponse), "You are a strategic planner focused on continuous improvement.");
                    iterationResult.ConversationHistory.Add($"Planner: {plannerResponse}");
                    iterationResult.Plan = plannerResponse;

                    // Maker
                    var makerResponse = await RunAgentAsync("MakerAgent", MakerPrompt(currentIteration, plannerResponse), "You are a maker agent focused on execution and implementation.");
                    iterationResult.ConversationHistory.Add($"Maker: {makerResponse}");
                    iterationResult.Execution = makerResponse;

                    // Checker
                    var checkerResponse = await RunAgentAsync("CheckerAgent", CheckerPrompt(currentIteration, makerResponse), "You are a quality checker focused on continuous improvement.");
                    iterationResult.ConversationHistory.Add($"Checker: {checkerResponse}");
                    iterationResult.Evaluation = checkerResponse;

                    // Reflector
                    var reflectorResponse = await RunAgentAsync("ReflectorAgent", ReflectorPrompt(currentIteration, checkerResponse, iterationResult), "You are a strategic reflector focused on meta-learning and convergence.");
                    iterationResult.ConversationHistory.Add($"Reflector: {reflectorResponse}");
                    iterationResult.Reflection = reflectorResponse;

                    // Convergence
                    var convergenceDecision = ExtractConvergenceDecision(reflectorResponse);
                    var newConvergenceScore = ExtractConvergenceScore(reflectorResponse);
                    iterationResult.ConvergenceScore = newConvergenceScore;
                    iterationResult.ShouldContinue = convergenceDecision;
                    iterationResult.EndTime = DateTime.UtcNow;
                    iterationResult.Duration = iterationResult.EndTime - iterationResult.StartTime;
                    iterationResult.Summary = $"Convergence: {newConvergenceScore:F2}, Duration: {iterationResult.Duration.TotalSeconds:F1}s, Continue: {convergenceDecision}";
                    Iterations.Add(iterationResult);
                    convergenceScore = newConvergenceScore;
                    shouldContinue = convergenceDecision && newConvergenceScore < _convergenceThreshold;
                    _logger.LogSystemMessage($"✅ Iteration {currentIteration} completed. Convergence: {newConvergenceScore:F2}, Continue: {shouldContinue}", Color.Green);
                    if (shouldContinue && currentIteration < _maxIterations)
                        await Task.Delay(2000);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Iteration {currentIteration} failed: {ex.Message}");
                    iterationResult.EndTime = DateTime.UtcNow;
                    iterationResult.Duration = iterationResult.EndTime - iterationResult.StartTime;
                    iterationResult.Summary = $"FAILED: {ex.Message}";
                    Iterations.Add(iterationResult);
                    shouldContinue = false;
                }
            }
            FinalConvergenceScore = convergenceScore;
            Converged = !shouldContinue;
            _logger.LogSystemMessage($"✅ Converged after {currentIteration} iterations! Final convergence score: {convergenceScore:F2}", Color.Green);
        }

        private async Task<string> RunAgentAsync(string agentName, string prompt, string system)
        {
            _logger.LogAgentMessage(agentName, agentName.ToLower(), prompt);
            var request = new OllamaRequest { Model = _model, Prompt = prompt, System = system, Stream = false };
            var response = await _ollamaService.SendMessageAsync(request);
            _logger.LogAgentMessage(agentName, agentName.ToLower(), response.Response);
            return response.Response;
        }

        // --- PROMPT BUILDERS ---
        private string OrchestratorPrompt(int iteration, double convergenceScore) => $@"You are the Master Orchestrator of a continuous self-evolving AI system.\nTask: {_task}\nIteration: {iteration}\nCurrent convergence score: {convergenceScore:F2}\nDecide if we should continue iterating or converge. If continuing, provide refined approach.";
        private string PlannerPrompt(int iteration, string orchestratorResponse) => $@"You are a Strategic Planner.\nTask: {_task}\nIteration: {iteration}\nOrchestrator's decision: {orchestratorResponse}\nCreate or refine the plan based on what we've learned.";
        private string MakerPrompt(int iteration, string plannerResponse) => $@"You are a Maker Agent.\nTask: {_task}\nIteration: {iteration}\nCurrent plan: {plannerResponse}\nExecute the current plan with attention to what we've learned.";
        private string CheckerPrompt(int iteration, string makerResponse) => $@"You are a Quality Checker.\nTask: {_task}\nIteration: {iteration}\nCurrent execution: {makerResponse}\nEvaluate the quality and identify areas for improvement.";
        private string ReflectorPrompt(int iteration, string checkerResponse, IterationResult iterationResult) => $@"You are a Strategic Reflector.\nTask: {_task}\nIteration: {iteration}\nCurrent evaluation: {checkerResponse}\nFull iteration history: {string.Join("\n", iterationResult.ConversationHistory)}\nAnalyze the entire conversation and decide:\n1. Should we continue to another iteration?\n2. What convergence score would you give (0.0-1.0)?\n3. What meta-improvements can we make to the system itself?\n4. What are the key insights learned?";

        // --- CONVERGENCE EXTRACTION ---
        private static bool ExtractConvergenceDecision(string response)
        {
            var lowerResponse = response.ToLower();
            if (lowerResponse.Contains("continue") && lowerResponse.Contains("yes"))
                return true;
            if (lowerResponse.Contains("converge") && lowerResponse.Contains("no"))
                return false;
            if (lowerResponse.Contains("stop") || lowerResponse.Contains("end"))
                return false;
            return true;
        }

        private static double ExtractConvergenceScore(string response)
        {
            var match = System.Text.RegularExpressions.Regex.Match(response, @"convergence.*?(\d+\.?\d*)");
            if (match.Success && double.TryParse(match.Groups[1].Value, out var score))
            {
                return Math.Max(0.0, Math.Min(1.0, score));
            }
            match = System.Text.RegularExpressions.Regex.Match(response, @"(\d+)%");
            if (match.Success && double.TryParse(match.Groups[1].Value, out var percentage))
            {
                return Math.Max(0.0, Math.Min(1.0, percentage / 100.0));
            }
            var lowerResponse = response.ToLower();
            if (lowerResponse.Contains("excellent") || lowerResponse.Contains("perfect"))
                return 0.9;
            if (lowerResponse.Contains("good") || lowerResponse.Contains("satisfactory"))
                return 0.7;
            if (lowerResponse.Contains("needs improvement") || lowerResponse.Contains("continue"))
                return 0.4;
            return 0.5;
        }
    }

    [HttpPost("continuous-loop-prod")]
    public async Task<IActionResult> StartContinuousLoopProd([FromBody] ContinuousLoopRequest request)
    {
        try
        {
            var conversationalLogger = HttpContext.RequestServices.GetRequiredService<IConversationalLogger>();
            var ollamaService = HttpContext.RequestServices.GetService<IOllamaService>();
            conversationalLogger.LogConversationStart("Continuous Loop Conversation", request.Task ?? "General discussion");
            if (ollamaService == null)
            {
                conversationalLogger.LogError("Ollama service not available. Please ensure Ollama is running and configured.");
                return BadRequest(new { success = false, error = "Ollama service not available. Please ensure Ollama is running and configured.", timestamp = DateTime.UtcNow });
            }
            var task = request.Task ?? "Design a microservices architecture";
            var maxIterations = request.MaxIterations ?? 5;
            var convergenceThreshold = request.ConvergenceThreshold ?? 0.8;
            var workflow = new ContinuousLoopWorkflow(ollamaService, conversationalLogger, ollamaService.Config.DefaultModel, task, maxIterations, convergenceThreshold);
            await workflow.RunAsync();
            conversationalLogger.LogConversationEnd("Continuous Loop Conversation", $"Continuous loop completed after {workflow.TotalIterations} iterations. Final convergence score: {workflow.FinalConvergenceScore:F2}.");
            return Ok(new
            {
                success = true,
                message = "Continuous loop conversation completed successfully (production workflow)",
                task = task,
                totalIterations = workflow.TotalIterations,
                finalConvergenceScore = workflow.FinalConvergenceScore,
                converged = workflow.Converged,
                iterations = workflow.Iterations,
                conversationHistory = workflow.ConversationLog,
                model = ollamaService.Config.DefaultModel,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            var conversationalLogger = HttpContext.RequestServices.GetRequiredService<IConversationalLogger>();
            conversationalLogger.LogError($"Continuous loop conversation failed: {ex.Message}");
            return BadRequest(new { success = false, error = ex.Message, timestamp = DateTime.UtcNow });
        }
    }
} 