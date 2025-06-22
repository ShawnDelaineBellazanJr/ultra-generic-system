using Microsoft.AspNetCore.Mvc;
using UltraGenericSystem.Models;
using UltraGenericSystem.Services;

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

    public AdvancedOrchestrationController(
        IAgentOrchestrator orchestrator,
        ILogger<AdvancedOrchestrationController> logger)
    {
        _orchestrator = orchestrator;
        _logger = logger;
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
} 