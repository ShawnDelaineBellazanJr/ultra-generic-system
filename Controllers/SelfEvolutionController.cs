using Microsoft.AspNetCore.Mvc;
using UltraGenericSystem.Models.SelfEvolution;
using UltraGenericSystem.Services.SelfEvolution;

namespace UltraGenericSystem.Controllers;

/// <summary>
/// Controller for self-evolution and meta-programming features
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SelfEvolutionController : ControllerBase
{
    private readonly ICodeGenerationService _codeGenerationService;
    private readonly IDynamicPluginService _pluginService;
    private readonly ILogger<SelfEvolutionController> _logger;

    public SelfEvolutionController(
        ICodeGenerationService codeGenerationService,
        IDynamicPluginService pluginService,
        ILogger<SelfEvolutionController> logger)
    {
        _codeGenerationService = codeGenerationService;
        _pluginService = pluginService;
        _logger = logger;
    }

    #region Code Generation Endpoints

    /// <summary>
    /// Generate code from template
    /// </summary>
    [HttpPost("generate")]
    public async Task<ActionResult<CodeGenerationResult>> GenerateCode([FromBody] CodeGenerationRequest request)
    {
        try
        {
            var result = await _codeGenerationService.GenerateCodeAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating code");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Compile source code
    /// </summary>
    [HttpPost("compile")]
    public async Task<ActionResult<CompilationResult>> CompileCode([FromBody] CompileCodeRequest request)
    {
        try
        {
            var result = await _codeGenerationService.CompileCodeAsync(
                request.SourceCode, 
                request.AssemblyName, 
                request.References);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error compiling code");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Analyze code for metrics and suggestions
    /// </summary>
    [HttpPost("analyze")]
    public async Task<ActionResult<CodeAnalysisResult>> AnalyzeCode([FromBody] AnalyzeCodeRequest request)
    {
        try
        {
            var result = await _codeGenerationService.AnalyzeCodeAsync(request.SourceCode, request.FilePath);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing code");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get code templates
    /// </summary>
    [HttpGet("templates")]
    public async Task<ActionResult<List<CodeTemplate>>> GetTemplates([FromQuery] string? templateType)
    {
        try
        {
            var templates = await _codeGenerationService.GetTemplatesAsync(templateType);
            return Ok(templates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting templates");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Save code template
    /// </summary>
    [HttpPost("templates")]
    public async Task<ActionResult<CodeTemplate>> SaveTemplate([FromBody] CodeTemplate template)
    {
        try
        {
            var savedTemplate = await _codeGenerationService.SaveTemplateAsync(template);
            return Ok(savedTemplate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving template");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Delete code template
    /// </summary>
    [HttpDelete("templates/{templateName}")]
    public async Task<ActionResult<bool>> DeleteTemplate(string templateName)
    {
        try
        {
            var result = await _codeGenerationService.DeleteTemplateAsync(templateName);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting template");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Generate entity class
    /// </summary>
    [HttpPost("generate/entity")]
    public async Task<ActionResult<CodeGenerationResult>> GenerateEntity([FromBody] GenerateEntityRequest request)
    {
        try
        {
            var result = await _codeGenerationService.GenerateEntityAsync(request.EntityName, request.Properties);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating entity");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Generate controller for entity
    /// </summary>
    [HttpPost("generate/controller")]
    public async Task<ActionResult<CodeGenerationResult>> GenerateController([FromBody] GenerateControllerRequest request)
    {
        try
        {
            var result = await _codeGenerationService.GenerateControllerAsync(request.EntityName, request.EntityType);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating controller");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Generate service for entity
    /// </summary>
    [HttpPost("generate/service")]
    public async Task<ActionResult<CodeGenerationResult>> GenerateService([FromBody] GenerateServiceRequest request)
    {
        try
        {
            var result = await _codeGenerationService.GenerateServiceAsync(request.EntityName, request.EntityType);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating service");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Generate repository for entity
    /// </summary>
    [HttpPost("generate/repository")]
    public async Task<ActionResult<CodeGenerationResult>> GenerateRepository([FromBody] GenerateRepositoryRequest request)
    {
        try
        {
            var result = await _codeGenerationService.GenerateRepositoryAsync(request.EntityName, request.EntityType);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating repository");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Generate complete CRUD stack
    /// </summary>
    [HttpPost("generate/crud-stack")]
    public async Task<ActionResult<List<CodeGenerationResult>>> GenerateCrudStack([FromBody] GenerateCrudStackRequest request)
    {
        try
        {
            var results = await _codeGenerationService.GenerateCrudStackAsync(request.EntityName, request.Properties);
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating CRUD stack");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Validate code
    /// </summary>
    [HttpPost("validate")]
    public async Task<ActionResult<bool>> ValidateCode([FromBody] ValidateCodeRequest request)
    {
        try
        {
            var result = await _codeGenerationService.ValidateCodeAsync(request.SourceCode);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating code");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get code generation statistics
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<Dictionary<string, object>>> GetStatistics()
    {
        try
        {
            var stats = await _codeGenerationService.GetStatisticsAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting statistics");
            return BadRequest(new { error = ex.Message });
        }
    }

    #endregion

    #region Dynamic Plugin Endpoints

    /// <summary>
    /// Register a new plugin
    /// </summary>
    [HttpPost("plugins")]
    public async Task<ActionResult<DynamicPlugin>> RegisterPlugin([FromBody] DynamicPlugin plugin)
    {
        try
        {
            var registeredPlugin = await _pluginService.RegisterPluginAsync(plugin);
            return Ok(registeredPlugin);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering plugin");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get all plugins
    /// </summary>
    [HttpGet("plugins")]
    public async Task<ActionResult<List<DynamicPlugin>>> GetPlugins([FromQuery] string? pluginType)
    {
        try
        {
            var plugins = await _pluginService.GetPluginsAsync(pluginType);
            return Ok(plugins);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting plugins");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get specific plugin
    /// </summary>
    [HttpGet("plugins/{pluginName}")]
    public async Task<ActionResult<DynamicPlugin>> GetPlugin(string pluginName)
    {
        try
        {
            var plugin = await _pluginService.GetPluginAsync(pluginName);
            if (plugin == null)
                return NotFound();
            return Ok(plugin);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting plugin");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update plugin
    /// </summary>
    [HttpPut("plugins")]
    public async Task<ActionResult<DynamicPlugin>> UpdatePlugin([FromBody] DynamicPlugin plugin)
    {
        try
        {
            var updatedPlugin = await _pluginService.UpdatePluginAsync(plugin);
            return Ok(updatedPlugin);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating plugin");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Delete plugin
    /// </summary>
    [HttpDelete("plugins/{pluginName}")]
    public async Task<ActionResult<bool>> DeletePlugin(string pluginName)
    {
        try
        {
            var result = await _pluginService.DeletePluginAsync(pluginName);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting plugin");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Load plugin
    /// </summary>
    [HttpPost("plugins/{pluginName}/load")]
    public async Task<ActionResult<bool>> LoadPlugin(string pluginName)
    {
        try
        {
            var result = await _pluginService.LoadPluginAsync(pluginName);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading plugin");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Unload plugin
    /// </summary>
    [HttpPost("plugins/{pluginName}/unload")]
    public async Task<ActionResult<bool>> UnloadPlugin(string pluginName)
    {
        try
        {
            var result = await _pluginService.UnloadPluginAsync(pluginName);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unloading plugin");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Compile and register plugin from source
    /// </summary>
    [HttpPost("plugins/compile")]
    public async Task<ActionResult<DynamicPlugin>> CompileAndRegisterPlugin([FromBody] CompilePluginRequest request)
    {
        try
        {
            var plugin = await _pluginService.CompileAndRegisterPluginAsync(
                request.PluginName, 
                request.SourceCode, 
                request.PluginType);
            return Ok(plugin);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error compiling and registering plugin");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Execute plugin method
    /// </summary>
    [HttpPost("plugins/{pluginName}/execute")]
    public async Task<ActionResult<object>> ExecutePluginMethod(string pluginName, [FromBody] ExecutePluginRequest request)
    {
        try
        {
            var result = await _pluginService.ExecutePluginMethodAsync(pluginName, request.MethodName, request.Parameters);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing plugin method");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get plugin statistics
    /// </summary>
    [HttpGet("plugins/statistics")]
    public async Task<ActionResult<Dictionary<string, object>>> GetPluginStatistics()
    {
        try
        {
            var stats = await _pluginService.GetPluginStatisticsAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting plugin statistics");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Validate plugin source code
    /// </summary>
    [HttpPost("plugins/validate")]
    public async Task<ActionResult<bool>> ValidatePluginSource([FromBody] ValidatePluginRequest request)
    {
        try
        {
            var result = await _pluginService.ValidatePluginSourceAsync(request.SourceCode);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating plugin source");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get plugin dependencies
    /// </summary>
    [HttpGet("plugins/{pluginName}/dependencies")]
    public async Task<ActionResult<List<string>>> GetPluginDependencies(string pluginName)
    {
        try
        {
            var dependencies = await _pluginService.GetPluginDependenciesAsync(pluginName);
            return Ok(dependencies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting plugin dependencies");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Check plugin compatibility
    /// </summary>
    [HttpGet("plugins/{pluginName}/compatibility")]
    public async Task<ActionResult<bool>> CheckPluginCompatibility(string pluginName)
    {
        try
        {
            var result = await _pluginService.CheckPluginCompatibilityAsync(pluginName);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking plugin compatibility");
            return BadRequest(new { error = ex.Message });
        }
    }

    #endregion
}

#region Request Models

public class CompileCodeRequest
{
    public string SourceCode { get; set; } = string.Empty;
    public string AssemblyName { get; set; } = string.Empty;
    public List<string>? References { get; set; }
}

public class AnalyzeCodeRequest
{
    public string SourceCode { get; set; } = string.Empty;
    public string? FilePath { get; set; }
}

public class GenerateEntityRequest
{
    public string EntityName { get; set; } = string.Empty;
    public Dictionary<string, string> Properties { get; set; } = new();
}

public class GenerateControllerRequest
{
    public string EntityName { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
}

public class GenerateServiceRequest
{
    public string EntityName { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
}

public class GenerateRepositoryRequest
{
    public string EntityName { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
}

public class GenerateCrudStackRequest
{
    public string EntityName { get; set; } = string.Empty;
    public Dictionary<string, string> Properties { get; set; } = new();
}

public class ValidateCodeRequest
{
    public string SourceCode { get; set; } = string.Empty;
}

public class CompilePluginRequest
{
    public string PluginName { get; set; } = string.Empty;
    public string SourceCode { get; set; } = string.Empty;
    public string PluginType { get; set; } = string.Empty;
}

public class ExecutePluginRequest
{
    public string MethodName { get; set; } = string.Empty;
    public object[] Parameters { get; set; } = Array.Empty<object>();
}

public class ValidatePluginRequest
{
    public string SourceCode { get; set; } = string.Empty;
}

#endregion 