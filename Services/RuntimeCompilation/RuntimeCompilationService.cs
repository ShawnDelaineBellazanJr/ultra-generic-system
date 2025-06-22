using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Loader;
using UltraGenericSystem.Models;

namespace UltraGenericSystem.Services.RuntimeCompilation;

/// <summary>
/// Implementation of runtime code compilation using Roslyn
/// </summary>
public class RuntimeCompilationService : IRuntimeCompilationService
{
    private readonly ILogger<RuntimeCompilationService> _logger;
    private readonly ConcurrentDictionary<string, AssemblyLoadContext> _assemblyContexts;
    private readonly ConcurrentDictionary<string, Assembly> _loadedAssemblies;
    private readonly CompilationStatistics _statistics;
    private readonly List<MetadataReference> _defaultReferences;

    public RuntimeCompilationService(ILogger<RuntimeCompilationService> logger)
    {
        _logger = logger;
        _assemblyContexts = new ConcurrentDictionary<string, AssemblyLoadContext>();
        _loadedAssemblies = new ConcurrentDictionary<string, Assembly>();
        _statistics = new CompilationStatistics();
        _defaultReferences = InitializeDefaultReferences();
    }

    /// <summary>
    /// Compiles C# code string to an assembly
    /// </summary>
    public async Task<CompilationResult> CompileCodeAsync(
        string sourceCode,
        string assemblyName,
        CompilationOptions options,
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        _logger.LogInformation("Starting compilation of assembly {AssemblyName}", assemblyName);

        try
        {
            // Validate code first
            var validationResult = await ValidateCodeAsync(sourceCode, new CodeValidationOptions(), cancellationToken);
            if (!validationResult.IsValid)
            {
                return new CompilationResult
                {
                    Success = false,
                    Errors = validationResult.Issues.Select(i => i.Message).ToList(),
                    CompilationTime = DateTime.UtcNow - startTime
                };
            }

            // Parse source code
            var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode, cancellationToken: cancellationToken);
            
            // Create compilation
            var compilation = CreateCompilation(syntaxTree, assemblyName, options);
            
            // Compile to memory
            using var memoryStream = new MemoryStream();
            var emitResult = compilation.Emit(memoryStream, cancellationToken: cancellationToken);

            if (!emitResult.Success)
            {
                var errors = emitResult.Diagnostics
                    .Where(d => d.Severity == DiagnosticSeverity.Error)
                    .Select(d => d.GetMessage())
                    .ToList();

                var warnings = emitResult.Diagnostics
                    .Where(d => d.Severity == DiagnosticSeverity.Warning)
                    .Select(d => d.GetMessage())
                    .ToList();

                UpdateStatistics(false, DateTime.UtcNow - startTime, 0);
                
                return new CompilationResult
                {
                    Success = false,
                    Errors = errors,
                    Warnings = warnings,
                    Diagnostics = emitResult.Diagnostics.ToList(),
                    CompilationTime = DateTime.UtcNow - startTime
                };
            }

            var assemblyBytes = memoryStream.ToArray();
            UpdateStatistics(true, DateTime.UtcNow - startTime, assemblyBytes.Length);

            _logger.LogInformation("Successfully compiled assembly {AssemblyName} ({Size} bytes)", 
                assemblyName, assemblyBytes.Length);

            return new CompilationResult
            {
                Success = true,
                AssemblyBytes = assemblyBytes,
                CompilationTime = DateTime.UtcNow - startTime,
                AssemblySize = assemblyBytes.Length,
                Metadata = new Dictionary<string, object>
                {
                    ["assemblyName"] = assemblyName,
                    ["sourceCodeLength"] = sourceCode.Length,
                    ["linesOfCode"] = sourceCode.Split('\n').Length
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error compiling assembly {AssemblyName}", assemblyName);
            UpdateStatistics(false, DateTime.UtcNow - startTime, 0);
            
            return new CompilationResult
            {
                Success = false,
                Errors = new List<string> { ex.Message },
                CompilationTime = DateTime.UtcNow - startTime
            };
        }
    }

    /// <summary>
    /// Compiles and loads a skill/plugin class
    /// </summary>
    public async Task<SkillCompilationResult> CompileSkillAsync(
        string skillCode,
        string skillName,
        CompilationOptions options,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Compiling skill {SkillName}", skillName);

        // Ensure skill code has proper structure
        var enhancedSkillCode = EnsureSkillStructure(skillCode, skillName);
        
        var compilationResult = await CompileCodeAsync(enhancedSkillCode, $"{skillName}Skill", options, cancellationToken);
        
        if (!compilationResult.Success)
        {
            return new SkillCompilationResult
            {
                Success = false,
                Errors = compilationResult.Errors,
                Warnings = compilationResult.Warnings,
                CompilationTime = compilationResult.CompilationTime
            };
        }

        try
        {
            // Load assembly
            var assembly = LoadAssembly(compilationResult.AssemblyBytes!, $"{skillName}Skill");
            
            // Find skill class
            var skillType = FindSkillClass(assembly, skillName);
            if (skillType == null)
            {
                return new SkillCompilationResult
                {
                    Success = false,
                    Errors = new List<string> { $"Could not find skill class '{skillName}' in compiled assembly" },
                    CompilationTime = compilationResult.CompilationTime
                };
            }

            // Create instance
            var skillInstance = Activator.CreateInstance(skillType);
            
            // Extract function information
            var functions = ExtractSkillFunctions(skillType);

            _logger.LogInformation("Successfully compiled and loaded skill {SkillName} with {FunctionCount} functions", 
                skillName, functions.Count);

            return new SkillCompilationResult
            {
                Success = true,
                AssemblyBytes = compilationResult.AssemblyBytes,
                CompilationTime = compilationResult.CompilationTime,
                AssemblySize = compilationResult.AssemblySize,
                SkillName = skillName,
                SkillType = skillType,
                SkillInstance = skillInstance,
                AvailableFunctions = functions.Select(f => f.Name).ToList(),
                FunctionDescriptions = functions.ToDictionary(f => f.Name, f => f.Description),
                Metadata = compilationResult.Metadata
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading skill {SkillName}", skillName);
            return new SkillCompilationResult
            {
                Success = false,
                Errors = new List<string> { ex.Message },
                CompilationTime = compilationResult.CompilationTime
            };
        }
    }

    /// <summary>
    /// Validates code for security and safety
    /// </summary>
    public async Task<CodeValidationResult> ValidateCodeAsync(
        string sourceCode,
        CodeValidationOptions options,
        CancellationToken cancellationToken = default)
    {
        var issues = new List<ValidationIssue>();
        var securityWarnings = new List<string>();
        var performanceWarnings = new List<string>();

        try
        {
            // Parse code
            var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode, cancellationToken: cancellationToken);
            var root = await syntaxTree.GetRootAsync(cancellationToken);

            // Security validation
            if (options.EnableSecurityValidation)
            {
                var securityIssues = ValidateSecurity(root, options);
                issues.AddRange(securityIssues);
                securityWarnings.AddRange(securityIssues.Where(i => i.Severity == ValidationSeverity.Warning).Select(i => i.Message));
            }

            // Performance validation
            if (options.EnablePerformanceValidation)
            {
                var performanceIssues = ValidatePerformance(root, options);
                issues.AddRange(performanceIssues);
                performanceWarnings.AddRange(performanceIssues.Where(i => i.Severity == ValidationSeverity.Warning).Select(i => i.Message));
            }

            // Complexity validation
            var complexityIssues = ValidateComplexity(root, options);
            issues.AddRange(complexityIssues);

            // Error handling validation
            if (options.RequireErrorHandling)
            {
                var errorHandlingIssues = ValidateErrorHandling(root);
                issues.AddRange(errorHandlingIssues);
            }

            var metrics = CalculateMetrics(root);

            return new CodeValidationResult
            {
                IsValid = !issues.Any(i => i.Severity == ValidationSeverity.Error || i.Severity == ValidationSeverity.Critical),
                Issues = issues,
                SecurityWarnings = securityWarnings,
                PerformanceWarnings = performanceWarnings,
                Metrics = metrics
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating code");
            return new CodeValidationResult
            {
                IsValid = false,
                Issues = new List<ValidationIssue>
                {
                    new ValidationIssue
                    {
                        Type = ValidationIssueType.Style,
                        Message = $"Code validation failed: {ex.Message}",
                        Severity = ValidationSeverity.Error
                    }
                }
            };
        }
    }

    /// <summary>
    /// Unloads a compiled assembly
    /// </summary>
    public async Task<bool> UnloadAssemblyAsync(string assemblyName, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_assemblyContexts.TryRemove(assemblyName, out var context))
            {
                context.Unload();
                _loadedAssemblies.TryRemove(assemblyName, out _);
                _statistics.LoadedAssemblies--;
                
                _logger.LogInformation("Successfully unloaded assembly {AssemblyName}", assemblyName);
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unloading assembly {AssemblyName}", assemblyName);
            return false;
        }
    }

    /// <summary>
    /// Gets compilation statistics
    /// </summary>
    public CompilationStatistics GetStatistics()
    {
        return _statistics;
    }

    #region Private Methods

    private List<MetadataReference> InitializeDefaultReferences()
    {
        var references = new List<MetadataReference>();

        // Core .NET references
        var coreAssemblies = new[]
        {
            typeof(object).Assembly,
            typeof(Console).Assembly,
            typeof(Enumerable).Assembly,
            typeof(Task).Assembly,
            typeof(Microsoft.SemanticKernel.Kernel).Assembly,
            typeof(Microsoft.SemanticKernel.KernelFunctionAttribute).Assembly
        };

        foreach (var assembly in coreAssemblies)
        {
            references.Add(MetadataReference.CreateFromFile(assembly.Location));
        }

        return references;
    }

    private CSharpCompilation CreateCompilation(SyntaxTree syntaxTree, string assemblyName, CompilationOptions options)
    {
        var references = new List<MetadataReference>(_defaultReferences);
        
        // Add additional references
        foreach (var reference in options.AdditionalReferences)
        {
            if (File.Exists(reference))
            {
                references.Add(MetadataReference.CreateFromFile(reference));
            }
        }

        var compilationOptions = new CSharpCompilationOptions(options.OutputKind)
            .WithOptimizationLevel(options.OptimizationLevel)
            .WithNullableContextOptions(options.EnableNullableReferenceTypes ? NullableContextOptions.Enable : NullableContextOptions.Disable)
            .WithAllowUnsafe(options.EnableUnsafeCode)
            .WithDeterministic(options.EnableDeterministicCompilation);

        return CSharpCompilation.Create(assemblyName)
            .AddSyntaxTrees(syntaxTree)
            .AddReferences(references)
            .WithOptions(compilationOptions);
    }

    private string EnsureSkillStructure(string skillCode, string skillName)
    {
        // Ensure the code has proper using statements and class structure
        var usingStatements = new[]
        {
            "using System;",
            "using System.Threading.Tasks;",
            "using Microsoft.SemanticKernel;",
            "using Microsoft.SemanticKernel.Kernel;"
        };

        var enhancedCode = string.Join("\n", usingStatements) + "\n\n" + skillCode;

        // Ensure class has KernelFunction attributes
        if (!enhancedCode.Contains("[KernelFunction]"))
        {
            _logger.LogWarning("Skill code for {SkillName} does not contain KernelFunction attributes", skillName);
        }

        return enhancedCode;
    }

    private Assembly LoadAssembly(byte[] assemblyBytes, string assemblyName)
    {
        var context = new AssemblyLoadContext(assemblyName, isCollectible: true);
        var assembly = context.LoadFromStream(new MemoryStream(assemblyBytes));
        
        _assemblyContexts[assemblyName] = context;
        _loadedAssemblies[assemblyName] = assembly;
        _statistics.LoadedAssemblies++;

        return assembly;
    }

    private Type? FindSkillClass(Assembly assembly, string skillName)
    {
        return assembly.GetTypes()
            .FirstOrDefault(t => t.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase) ||
                                t.Name.Equals($"{skillName}Skill", StringComparison.OrdinalIgnoreCase));
    }

    private List<SkillFunctionInfo> ExtractSkillFunctions(Type skillType)
    {
        var functions = new List<SkillFunctionInfo>();

        foreach (var method in skillType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
        {
            var kernelFunctionAttr = method.GetCustomAttribute<Microsoft.SemanticKernel.KernelFunctionAttribute>();
            if (kernelFunctionAttr != null)
            {
                functions.Add(new SkillFunctionInfo
                {
                    Name = method.Name,
                    Description = method.Name,
                    ReturnType = method.ReturnType,
                    Parameters = method.GetParameters().Select(p => p.Name ?? "unknown").ToList()
                });
            }
        }

        return functions;
    }

    private List<ValidationIssue> ValidateSecurity(SyntaxNode root, CodeValidationOptions options)
    {
        var issues = new List<ValidationIssue>();

        // Check for dangerous namespaces
        var dangerousNamespaces = new[] { "System.IO", "System.Net", "System.Diagnostics", "System.Reflection" };
        
        var usingDirectives = root.DescendantNodes().OfType<UsingDirectiveSyntax>();
        foreach (var usingDirective in usingDirectives)
        {
            var namespaceName = usingDirective.Name?.ToString();
            if (dangerousNamespaces.Any(dn => namespaceName?.StartsWith(dn) == true))
            {
                issues.Add(new ValidationIssue
                {
                    Type = ValidationIssueType.Security,
                    Message = $"Potentially dangerous namespace used: {namespaceName}",
                    Severity = ValidationSeverity.Warning,
                    LineNumber = usingDirective.GetLocation().GetLineSpan().StartLinePosition.Line + 1
                });
            }
        }

        // Check for dangerous method calls
        var dangerousMethods = new[] { "File.Delete", "Process.Start", "Environment.Exit" };
        var invocations = root.DescendantNodes().OfType<InvocationExpressionSyntax>();
        
        foreach (var invocation in invocations)
        {
            var methodName = invocation.ToString();
            if (dangerousMethods.Any(dm => methodName.Contains(dm)))
            {
                issues.Add(new ValidationIssue
                {
                    Type = ValidationIssueType.Security,
                    Message = $"Potentially dangerous method call: {methodName}",
                    Severity = ValidationSeverity.Error,
                    LineNumber = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1
                });
            }
        }

        return issues;
    }

    private List<ValidationIssue> ValidatePerformance(SyntaxNode root, CodeValidationOptions options)
    {
        var issues = new List<ValidationIssue>();

        // Check for potential performance issues
        var performancePatterns = new[]
        {
            "new List<",
            "new Dictionary<",
            "foreach",
            "for (",
            "while ("
        };

        var code = root.ToString();
        var lines = code.Split('\n');
        
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if (performancePatterns.Any(pattern => line.Contains(pattern)))
            {
                issues.Add(new ValidationIssue
                {
                    Type = ValidationIssueType.Performance,
                    Message = $"Potential performance consideration: {line.Trim()}",
                    Severity = ValidationSeverity.Info,
                    LineNumber = i + 1
                });
            }
        }

        return issues;
    }

    private List<ValidationIssue> ValidateComplexity(SyntaxNode root, CodeValidationOptions options)
    {
        var issues = new List<ValidationIssue>();

        // Simple complexity check - count control flow statements
        var controlFlowNodes = root.DescendantNodes().OfType<IfStatementSyntax>()
            .Concat<SyntaxNode>(root.DescendantNodes().OfType<ForStatementSyntax>())
            .Concat(root.DescendantNodes().OfType<WhileStatementSyntax>())
            .Concat(root.DescendantNodes().OfType<SwitchStatementSyntax>());

        var complexity = controlFlowNodes.Count();
        
        if (complexity > options.MaxComplexity)
        {
            issues.Add(new ValidationIssue
            {
                Type = ValidationIssueType.Complexity,
                Message = $"Code complexity ({complexity}) exceeds maximum allowed ({options.MaxComplexity})",
                Severity = ValidationSeverity.Warning
            });
        }

        return issues;
    }

    private List<ValidationIssue> ValidateErrorHandling(SyntaxNode root)
    {
        var issues = new List<ValidationIssue>();

        // Check for try-catch blocks
        var tryCatchBlocks = root.DescendantNodes().OfType<TryStatementSyntax>();
        var methodDeclarations = root.DescendantNodes().OfType<MethodDeclarationSyntax>();

        foreach (var method in methodDeclarations)
        {
            var hasTryCatch = method.DescendantNodes().OfType<TryStatementSyntax>().Any();
            if (!hasTryCatch)
            {
                issues.Add(new ValidationIssue
                {
                    Type = ValidationIssueType.ErrorHandling,
                    Message = $"Method '{method.Identifier.Text}' lacks error handling",
                    Severity = ValidationSeverity.Warning,
                    LineNumber = method.GetLocation().GetLineSpan().StartLinePosition.Line + 1
                });
            }
        }

        return issues;
    }

    private Dictionary<string, object> CalculateMetrics(SyntaxNode root)
    {
        var linesOfCode = root.ToString().Split('\n').Length;
        var methodCount = root.DescendantNodes().OfType<MethodDeclarationSyntax>().Count();
        var classCount = root.DescendantNodes().OfType<ClassDeclarationSyntax>().Count();
        var complexity = root.DescendantNodes().OfType<IfStatementSyntax>().Count() +
                        root.DescendantNodes().OfType<ForStatementSyntax>().Count() +
                        root.DescendantNodes().OfType<WhileStatementSyntax>().Count();

        return new Dictionary<string, object>
        {
            ["linesOfCode"] = linesOfCode,
            ["methodCount"] = methodCount,
            ["classCount"] = classCount,
            ["complexity"] = complexity
        };
    }

    private void UpdateStatistics(bool success, TimeSpan compilationTime, long assemblySize)
    {
        _statistics.TotalCompilations++;
        _statistics.TotalCompilationTime += compilationTime;
        _statistics.TotalAssemblySize += assemblySize;

        if (success)
        {
            _statistics.SuccessfulCompilations++;
        }
        else
        {
            _statistics.FailedCompilations++;
        }

        _statistics.AverageCompilationTime = TimeSpan.FromTicks(_statistics.TotalCompilationTime.Ticks / _statistics.TotalCompilations);
    }

    #endregion
}

/// <summary>
/// Skill function information
/// </summary>
public class SkillFunctionInfo
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Type ReturnType { get; set; } = null!;
    public List<string> Parameters { get; set; } = new();
} 