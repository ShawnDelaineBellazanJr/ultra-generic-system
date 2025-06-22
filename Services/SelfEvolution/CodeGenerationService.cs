using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UltraGenericSystem.Models;
using UltraGenericSystem.Models.SelfEvolution;
using UltraGenericSystem.Repositories;

namespace UltraGenericSystem.Services.SelfEvolution;

/// <summary>
/// Service for code generation and dynamic compilation using Roslyn
/// </summary>
public class CodeGenerationService : ICodeGenerationService
{
    private readonly IGenericRepository<CodeTemplate> _templateRepository;
    private readonly IGenericRepository<DynamicPlugin> _pluginRepository;
    private readonly IGenericRepository<SelfEvolutionConfig> _configRepository;
    private readonly ILogger<CodeGenerationService> _logger;
    private readonly Dictionary<string, string> _defaultTemplates;

    public CodeGenerationService(
        IGenericRepository<CodeTemplate> templateRepository,
        IGenericRepository<DynamicPlugin> pluginRepository,
        IGenericRepository<SelfEvolutionConfig> configRepository,
        ILogger<CodeGenerationService> logger)
    {
        _templateRepository = templateRepository;
        _pluginRepository = pluginRepository;
        _configRepository = configRepository;
        _logger = logger;
        _defaultTemplates = InitializeDefaultTemplates();
    }

    /// <summary>
    /// Generate code from a template with parameters
    /// </summary>
    public async Task<CodeGenerationResult> GenerateCodeAsync(CodeGenerationRequest request)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = new CodeGenerationResult();

        try
        {
            _logger.LogInformation("Generating code from template: {TemplateName}", request.TemplateName);

            // Get template
            var template = await GetTemplateAsync(request.TemplateName);
            if (template == null)
            {
                result.Success = false;
                result.Errors.Add($"Template '{request.TemplateName}' not found");
                return result;
            }

            // Process template
            var processedCode = ProcessTemplate(template.TemplateContent, request.Parameters);
            result.GeneratedCode = processedCode;

            // Save to file if output path specified
            if (!string.IsNullOrEmpty(request.OutputPath))
            {
                await SaveCodeToFileAsync(request.OutputPath, processedCode);
                result.OutputPath = request.OutputPath;
            }

            // Compile immediately if requested
            if (request.CompileImmediately)
            {
                var assemblyName = Path.GetFileNameWithoutExtension(request.OutputPath ?? "GeneratedAssembly");
                result.CompilationResult = await CompileCodeAsync(processedCode, assemblyName);
            }

            result.Success = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating code from template: {TemplateName}", request.TemplateName);
            result.Success = false;
            result.Errors.Add(ex.Message);
        }
        finally
        {
            stopwatch.Stop();
            result.GenerationTime = stopwatch.Elapsed;
        }

        return result;
    }

    /// <summary>
    /// Compile source code to assembly using Roslyn
    /// </summary>
    public async Task<CompilationResult> CompileCodeAsync(string sourceCode, string assemblyName, List<string>? references = null)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = new CompilationResult();

        try
        {
            _logger.LogInformation("Compiling code for assembly: {AssemblyName}", assemblyName);

            // Parse source code
            var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
            var root = await syntaxTree.GetRootAsync();
            var compilationUnit = root as CompilationUnitSyntax;

            if (compilationUnit == null)
            {
                result.Success = false;
                result.Diagnostics.Add(new DiagnosticInfo
                {
                    Id = "CS0001",
                    Message = "Invalid compilation unit",
                    Severity = "Error"
                });
                return result;
            }

            // Get default references
            var defaultReferences = GetDefaultReferences();
            if (references != null)
            {
                var additionalReferences = references.Select(r => MetadataReference.CreateFromFile(r)).ToList();
                defaultReferences.AddRange(additionalReferences);
            }

            // Create compilation
            var compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { syntaxTree },
                references: defaultReferences,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    .WithOptimizationLevel(OptimizationLevel.Release)
                    .WithNullableContextOptions(NullableContextOptions.Enable)
            );

            // Compile
            using var ms = new MemoryStream();
            var emitResult = compilation.Emit(ms);

            if (emitResult.Success)
            {
                result.Success = true;
                result.AssemblyBytes = ms.ToArray();
                result.AssemblyName = assemblyName;
                result.ReferencedAssemblies = defaultReferences.Select(r => r.Display).ToList();
            }
            else
            {
                result.Success = false;
                foreach (var diagnostic in emitResult.Diagnostics)
                {
                    var location = diagnostic.Location;
                    result.Diagnostics.Add(new DiagnosticInfo
                    {
                        Id = diagnostic.Id,
                        Message = diagnostic.GetMessage(),
                        Severity = diagnostic.Severity.ToString(),
                        Line = location.GetLineSpan().StartLinePosition.Line + 1,
                        Column = location.GetLineSpan().StartLinePosition.Character + 1,
                        FilePath = location.SourceTree?.FilePath
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error compiling code for assembly: {AssemblyName}", assemblyName);
            result.Success = false;
            result.Diagnostics.Add(new DiagnosticInfo
            {
                Id = "CS9999",
                Message = ex.Message,
                Severity = "Error"
            });
        }
        finally
        {
            stopwatch.Stop();
            result.CompilationTime = stopwatch.Elapsed;
        }

        return result;
    }

    /// <summary>
    /// Analyze code for metrics, issues, and suggestions
    /// </summary>
    public async Task<CodeAnalysisResult> AnalyzeCodeAsync(string sourceCode, string? filePath = null)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = new CodeAnalysisResult();

        try
        {
            _logger.LogInformation("Analyzing code: {FilePath}", filePath ?? "Unknown");

            // Parse source code
            var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
            var root = await syntaxTree.GetRootAsync();

            // Analyze syntax
            var analyzer = new CodeAnalyzer();
            var analysis = analyzer.Analyze(root);

            result.Success = true;
            result.Metrics = analysis.Metrics;
            result.Suggestions = analysis.Suggestions;
            result.Issues = analysis.Issues;
            result.AnalysisData = analysis.AnalysisData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing code: {FilePath}", filePath ?? "Unknown");
            result.Success = false;
        }
        finally
        {
            stopwatch.Stop();
            result.AnalysisTime = stopwatch.Elapsed;
        }

        return result;
    }

    /// <summary>
    /// Get available code templates
    /// </summary>
    public async Task<List<CodeTemplate>> GetTemplatesAsync(string? templateType = null)
    {
        var templates = await _templateRepository.GetAllAsync();
        
        if (!string.IsNullOrEmpty(templateType))
        {
            templates = templates.Where(t => t.TemplateType == templateType);
        }

        return templates.ToList();
    }

    /// <summary>
    /// Create or update a code template
    /// </summary>
    public async Task<CodeTemplate> SaveTemplateAsync(CodeTemplate template)
    {
        var existing = await _templateRepository.QueryAsync(t => t.Name == template.Name);
        var existingTemplate = existing.FirstOrDefault();
        
        if (existingTemplate != null)
        {
            template.Id = existingTemplate.Id;
            template.Version = existingTemplate.Version + 1;
            return await _templateRepository.UpdateAsync(template);
        }
        else
        {
            return await _templateRepository.CreateAsync(template);
        }
    }

    /// <summary>
    /// Delete a code template
    /// </summary>
    public async Task<bool> DeleteTemplateAsync(string templateName)
    {
        var templates = await _templateRepository.QueryAsync(t => t.Name == templateName);
        var template = templates.FirstOrDefault();
        if (template != null)
        {
            await _templateRepository.DeleteAsync(template.Id);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Generate entity class from schema
    /// </summary>
    public async Task<CodeGenerationResult> GenerateEntityAsync(string entityName, Dictionary<string, string> properties)
    {
        var parameters = new Dictionary<string, object>
        {
            ["EntityName"] = entityName,
            ["Properties"] = properties,
            ["Namespace"] = "UltraGenericSystem.Models.Generated"
        };

        var request = new CodeGenerationRequest
        {
            TemplateName = "Entity",
            Parameters = parameters,
            OutputPath = $"Models/Generated/{entityName}.cs",
            CompileImmediately = true
        };

        return await GenerateCodeAsync(request);
    }

    /// <summary>
    /// Generate controller for an entity
    /// </summary>
    public async Task<CodeGenerationResult> GenerateControllerAsync(string entityName, string entityType)
    {
        var parameters = new Dictionary<string, object>
        {
            ["EntityName"] = entityName,
            ["EntityType"] = entityType,
            ["Namespace"] = "UltraGenericSystem.Controllers.Generated"
        };

        var request = new CodeGenerationRequest
        {
            TemplateName = "Controller",
            Parameters = parameters,
            OutputPath = $"Controllers/Generated/{entityName}Controller.cs",
            CompileImmediately = true
        };

        return await GenerateCodeAsync(request);
    }

    /// <summary>
    /// Generate service for an entity
    /// </summary>
    public async Task<CodeGenerationResult> GenerateServiceAsync(string entityName, string entityType)
    {
        var parameters = new Dictionary<string, object>
        {
            ["EntityName"] = entityName,
            ["EntityType"] = entityType,
            ["Namespace"] = "UltraGenericSystem.Services.Generated"
        };

        var request = new CodeGenerationRequest
        {
            TemplateName = "Service",
            Parameters = parameters,
            OutputPath = $"Services/Generated/{entityName}Service.cs",
            CompileImmediately = true
        };

        return await GenerateCodeAsync(request);
    }

    /// <summary>
    /// Generate repository for an entity
    /// </summary>
    public async Task<CodeGenerationResult> GenerateRepositoryAsync(string entityName, string entityType)
    {
        var parameters = new Dictionary<string, object>
        {
            ["EntityName"] = entityName,
            ["EntityType"] = entityType,
            ["Namespace"] = "UltraGenericSystem.Repositories.Generated"
        };

        var request = new CodeGenerationRequest
        {
            TemplateName = "Repository",
            Parameters = parameters,
            OutputPath = $"Repositories/Generated/{entityName}Repository.cs",
            CompileImmediately = true
        };

        return await GenerateCodeAsync(request);
    }

    /// <summary>
    /// Generate complete CRUD stack for an entity
    /// </summary>
    public async Task<List<CodeGenerationResult>> GenerateCrudStackAsync(string entityName, Dictionary<string, string> properties)
    {
        var results = new List<CodeGenerationResult>();

        // Generate entity
        var entityResult = await GenerateEntityAsync(entityName, properties);
        results.Add(entityResult);

        if (entityResult.Success)
        {
            // Generate repository
            var repoResult = await GenerateRepositoryAsync(entityName, entityName);
            results.Add(repoResult);

            // Generate service
            var serviceResult = await GenerateServiceAsync(entityName, entityName);
            results.Add(serviceResult);

            // Generate controller
            var controllerResult = await GenerateControllerAsync(entityName, entityName);
            results.Add(controllerResult);
        }

        return results;
    }

    /// <summary>
    /// Validate generated code
    /// </summary>
    public async Task<bool> ValidateCodeAsync(string sourceCode)
    {
        try
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
            var root = await syntaxTree.GetRootAsync();
            
            // Check for syntax errors
            var diagnostics = syntaxTree.GetDiagnostics();
            return !diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Get code generation statistics
    /// </summary>
    public async Task<Dictionary<string, object>> GetStatisticsAsync()
    {
        var templates = await _templateRepository.GetAllAsync();
        var plugins = await _pluginRepository.GetAllAsync();

        return new Dictionary<string, object>
        {
            ["TotalTemplates"] = templates.Count(),
            ["ActiveTemplates"] = templates.Count(t => t.IsActive),
            ["TotalPlugins"] = plugins.Count(),
            ["ActivePlugins"] = plugins.Count(p => p.IsActive),
            ["LoadedPlugins"] = plugins.Count(p => p.IsLoaded),
            ["TemplateTypes"] = templates.Select(t => t.TemplateType).Distinct().ToList(),
            ["PluginTypes"] = plugins.Select(p => p.PluginType).Distinct().ToList()
        };
    }

    #region Private Methods

    private async Task<CodeTemplate?> GetTemplateAsync(string templateName)
    {
        var templates = await _templateRepository.QueryAsync(t => t.Name == templateName && t.IsActive);
        var template = templates.FirstOrDefault();
        
        if (template == null && _defaultTemplates.ContainsKey(templateName))
        {
            // Create default template
            template = new CodeTemplate
            {
                Name = templateName,
                TemplateType = GetTemplateType(templateName),
                TemplateContent = _defaultTemplates[templateName],
                Description = $"Default {templateName} template",
                IsActive = true
            };
            
            template = await _templateRepository.CreateAsync(template);
        }

        return template;
    }

    private string ProcessTemplate(string template, Dictionary<string, object> parameters)
    {
        var result = template;

        foreach (var param in parameters)
        {
            var placeholder = $"{{{{{param.Key}}}}}";
            var value = param.Value?.ToString() ?? string.Empty;
            result = result.Replace(placeholder, value);
        }

        return result;
    }

    private async Task SaveCodeToFileAsync(string filePath, string content)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(filePath, content, Encoding.UTF8);
    }

    private List<MetadataReference> GetDefaultReferences()
    {
        var references = new List<MetadataReference>();

        // Add core assemblies
        var assemblies = new[]
        {
            typeof(object).Assembly,
            typeof(Console).Assembly,
            typeof(Enumerable).Assembly,
            typeof(Task).Assembly,
            typeof(BaseEntity).Assembly,
            typeof(IGenericRepository<>).Assembly
        };

        foreach (var assembly in assemblies)
        {
            references.Add(MetadataReference.CreateFromFile(assembly.Location));
        }

        return references;
    }

    private string GetTemplateType(string templateName)
    {
        return templateName.ToLower() switch
        {
            "entity" => "Entity",
            "controller" => "Controller",
            "service" => "Service",
            "repository" => "Repository",
            "skill" => "Skill",
            "function" => "Function",
            "agent" => "Agent",
            _ => "Custom"
        };
    }

    private Dictionary<string, string> InitializeDefaultTemplates()
    {
        return new Dictionary<string, string>
        {
            ["Entity"] = @"using System.ComponentModel.DataAnnotations;
using UltraGenericSystem.Models;

namespace {{Namespace}}
{
    public class {{EntityName}} : BaseEntity
    {
        {{#each Properties}}
        [MaxLength(255)]
        public {{Value}} {{Key}} { get; set; }
        {{/each}}
    }
}",

            ["Controller"] = @"using Microsoft.AspNetCore.Mvc;
using UltraGenericSystem.Models;
using UltraGenericSystem.Repositories;
using UltraGenericSystem.Services;

namespace {{Namespace}}
{
    [ApiController]
    [Route(""api/[controller]"")]
    public class {{EntityName}}Controller : ControllerBase
    {
        private readonly IGenericRepository<{{EntityType}}> _repository;
        private readonly ILogger<{{EntityName}}Controller> _logger;

        public {{EntityName}}Controller(IGenericRepository<{{EntityType}}> repository, ILogger<{{EntityName}}Controller> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<{{EntityType}}>>> GetAll()
        {
            var entities = await _repository.GetAllAsync();
            return Ok(entities);
        }

        [HttpGet(""{id}"")]
        public async Task<ActionResult<{{EntityType}}>> GetById(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                return NotFound();
            return Ok(entity);
        }

        [HttpPost]
        public async Task<ActionResult<{{EntityType}}>> Create([FromBody] {{EntityType}} entity)
        {
            var created = await _repository.CreateAsync(entity);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut(""{id}"")]
        public async Task<IActionResult> Update(int id, [FromBody] {{EntityType}} entity)
        {
            if (id != entity.Id)
                return BadRequest();
            
            await _repository.UpdateAsync(entity);
            return NoContent();
        }

        [HttpDelete(""{id}"")]
        public async Task<IActionResult> Delete(int id)
        {
            await _repository.DeleteAsync(id);
            return NoContent();
        }
    }
}",

            ["Service"] = @"using UltraGenericSystem.Models;
using UltraGenericSystem.Repositories;

namespace {{Namespace}}
{
    public interface I{{EntityName}}Service
    {
        Task<IEnumerable<{{EntityType}}>> GetAllAsync();
        Task<{{EntityType}}?> GetByIdAsync(int id);
        Task<{{EntityType}}> CreateAsync({{EntityType}} entity);
        Task<{{EntityType}}> UpdateAsync({{EntityType}} entity);
        Task DeleteAsync(int id);
    }

    public class {{EntityName}}Service : I{{EntityName}}Service
    {
        private readonly IGenericRepository<{{EntityType}}> _repository;
        private readonly ILogger<{{EntityName}}Service> _logger;

        public {{EntityName}}Service(IGenericRepository<{{EntityType}}> repository, ILogger<{{EntityName}}Service> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<IEnumerable<{{EntityType}}>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<{{EntityType}}?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<{{EntityType}}> CreateAsync({{EntityType}} entity)
        {
            return await _repository.CreateAsync(entity);
        }

        public async Task<{{EntityType}}> UpdateAsync({{EntityType}} entity)
        {
            return await _repository.UpdateAsync(entity);
        }

        public async Task DeleteAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }
    }
}",

            ["Repository"] = @"using UltraGenericSystem.Models;

namespace {{Namespace}}
{
    public interface I{{EntityName}}Repository : IGenericRepository<{{EntityType}}>
    {
        // Add custom repository methods here
    }

    public class {{EntityName}}Repository : GenericRepository<{{EntityType}}>, I{{EntityName}}Repository
    {
        public {{EntityName}}Repository(UltraGenericContext context) : base(context)
        {
        }

        // Implement custom repository methods here
    }
}"
        };
    }

    #endregion
}

/// <summary>
/// Code analyzer for metrics and suggestions
/// </summary>
public class CodeAnalyzer
{
    public CodeAnalysisResult Analyze(SyntaxNode root)
    {
        var result = new CodeAnalysisResult
        {
            Success = true,
            Metrics = new List<CodeMetric>(),
            Suggestions = new List<CodeSuggestion>(),
            Issues = new List<CodeIssue>(),
            AnalysisData = new Dictionary<string, object>()
        };

        // Analyze classes
        var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
        result.Metrics.Add(new CodeMetric
        {
            Name = "Classes",
            Value = classes.Count(),
            Unit = "count",
            Category = "Structure"
        });

        // Analyze methods
        var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>();
        result.Metrics.Add(new CodeMetric
        {
            Name = "Methods",
            Value = methods.Count(),
            Unit = "count",
            Category = "Structure"
        });

        // Analyze complexity
        var complexity = CalculateComplexity(root);
        result.Metrics.Add(new CodeMetric
        {
            Name = "CyclomaticComplexity",
            Value = complexity,
            Unit = "complexity",
            Category = "Complexity"
        });

        // Generate suggestions
        GenerateSuggestions(root, result);

        return result;
    }

    private double CalculateComplexity(SyntaxNode root)
    {
        var complexity = 1.0; // Base complexity

        // Count decision points
        complexity += root.DescendantNodes().OfType<IfStatementSyntax>().Count();
        complexity += root.DescendantNodes().OfType<SwitchStatementSyntax>().Count();
        complexity += root.DescendantNodes().OfType<ForStatementSyntax>().Count();
        complexity += root.DescendantNodes().OfType<WhileStatementSyntax>().Count();
        complexity += root.DescendantNodes().OfType<ForEachStatementSyntax>().Count();

        return complexity;
    }

    private void GenerateSuggestions(SyntaxNode root, CodeAnalysisResult result)
    {
        // Check for long methods
        var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>();
        foreach (var method in methods)
        {
            var lineCount = method.GetText().Lines.Count;
            if (lineCount > 20)
            {
                result.Suggestions.Add(new CodeSuggestion
                {
                    Title = "Long Method",
                    Description = $"Method '{method.Identifier.Text}' has {lineCount} lines. Consider breaking it into smaller methods.",
                    Category = "Maintainability",
                    Priority = "Medium",
                    LineNumber = method.GetLocation().GetLineSpan().StartLinePosition.Line + 1
                });
            }
        }

        // Check for missing XML documentation
        var publicMembers = root.DescendantNodes()
            .OfType<MemberDeclarationSyntax>()
            .Where(m => m.Modifiers.Any(mod => mod.IsKind(SyntaxKind.PublicKeyword)));

        foreach (var member in publicMembers)
        {
            if (!member.HasLeadingTrivia || !member.GetLeadingTrivia().Any(t => t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)))
            {
                result.Suggestions.Add(new CodeSuggestion
                {
                    Title = "Missing Documentation",
                    Description = $"Public member '{member.GetType().Name}' should have XML documentation.",
                    Category = "Documentation",
                    Priority = "Low",
                    LineNumber = member.GetLocation().GetLineSpan().StartLinePosition.Line + 1
                });
            }
        }
    }
} 