using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using UltraGenericSystem.Models;
using UltraGenericSystem.Services;
using UltraGenericSystem.Services.Agents;

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
} 