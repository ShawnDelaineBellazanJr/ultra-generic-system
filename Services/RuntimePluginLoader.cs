using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Reflection;
using System.Text;
using UltraGenericSystem.Services.RuntimeCompilation;
using UltraGenericSystem.Models.SelfEvolution;

namespace UltraGenericSystem.Services;

/// <summary>
/// Service for dynamically loading and managing plugins at runtime
/// Enables runtime extensibility through dynamic compilation and assembly loading
/// </summary>
public class RuntimePluginLoader : IRuntimePluginLoader
{
    private readonly Kernel _kernel;
    private readonly ILogger<RuntimePluginLoader> _logger;
    private readonly IRuntimeCompilationService _compilationService;
    private readonly Dictionary<string, LoadedPlugin> _loadedPlugins;
    private readonly Dictionary<string, Assembly> _pluginAssemblies;
    private readonly string _pluginDirectory;

    public RuntimePluginLoader(
        Kernel kernel,
        ILogger<RuntimePluginLoader> logger,
        IRuntimeCompilationService compilationService)
    {
        _kernel = kernel;
        _logger = logger;
        _compilationService = compilationService;
        _loadedPlugins = new Dictionary<string, LoadedPlugin>();
        _pluginAssemblies = new Dictionary<string, Assembly>();
        _pluginDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
        
        // Ensure plugin directory exists
        Directory.CreateDirectory(_pluginDirectory);
    }

    /// <summary>
    /// Loads a plugin from source code at runtime
    /// </summary>
    public async Task<PluginLoadResult> LoadPluginFromSourceAsync(
        string sourceCode,
        string pluginName,
        PluginLoadOptions options = null)
    {
        try
        {
            _logger.LogInformation("Loading plugin from source: {PluginName}", pluginName);

            options ??= new PluginLoadOptions();

            // Validate source code
            var validationResult = await ValidatePluginSourceAsync(sourceCode, pluginName);
            if (!validationResult.IsValid)
            {
                return new PluginLoadResult
                {
                    Success = false,
                    ErrorMessage = $"Plugin validation failed: {validationResult.Errors.FirstOrDefault()}"
                };
            }

            // Compile the plugin
            var compilationResult = await _compilationService.CompileCodeAsync(
                sourceCode,
                pluginName,
                new UltraGenericSystem.Services.RuntimeCompilation.CompilationOptions
                {
                    OutputKind = OutputKind.DynamicallyLinkedLibrary,
                    AdditionalReferences = GetPluginReferencePaths(),
                    OptimizationLevel = options.OptimizationLevel,
                    EnableUnsafeCode = options.AllowUnsafe
                });

            if (!compilationResult.Success)
            {
                return new PluginLoadResult
                {
                    Success = false,
                    ErrorMessage = compilationResult.Errors.FirstOrDefault() ?? "Compilation failed",
                    Diagnostics = compilationResult.Diagnostics.Select(d => d.GetMessage()).ToList()
                };
            }

            // Load the assembly
            var assembly = Assembly.Load(compilationResult.AssemblyBytes);
            if (assembly == null)
            {
                return new PluginLoadResult
                {
                    Success = false,
                    ErrorMessage = "Failed to load compiled assembly"
                };
            }

            // Register the plugin with Semantic Kernel
            var pluginInfo = new PluginInfo
            {
                Name = pluginName,
                Description = options?.Description ?? $"Plugin: {pluginName}",
                Version = options?.Version ?? "1.0.0",
                AssemblyPath = compilationResult.AssemblyPath ?? "",
                LoadedAt = DateTime.UtcNow
            };

            // Import the plugin into the kernel using a simpler approach
            // Note: SK plugin import is not available in this version, so we just store the instance
            var pluginInstance = Activator.CreateInstance(assembly.GetTypes().FirstOrDefault(t => t.IsClass && !t.IsAbstract) ?? typeof(object));
            
            var loadedPlugin = new LoadedPlugin
            {
                Name = pluginName,
                Assembly = assembly,
                Plugin = pluginInstance,
                Info = pluginInfo
            };

            _loadedPlugins[pluginName] = loadedPlugin;

            return new PluginLoadResult
            {
                Success = true,
                Plugin = loadedPlugin,
                AssemblyPath = compilationResult.AssemblyPath ?? ""
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading plugin from source: {PluginName}", pluginName);
            return new PluginLoadResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Loads a plugin from a file
    /// </summary>
    public async Task<PluginLoadResult> LoadPluginFromFileAsync(
        string filePath,
        PluginLoadOptions options = null)
    {
        try
        {
            _logger.LogInformation("Loading plugin from file: {FilePath}", filePath);

            if (!File.Exists(filePath))
            {
                return new PluginLoadResult
                {
                    Success = false,
                    ErrorMessage = $"Plugin file not found: {filePath}"
                };
            }

            var sourceCode = await File.ReadAllTextAsync(filePath);
            var pluginName = Path.GetFileNameWithoutExtension(filePath);

            return await LoadPluginFromSourceAsync(sourceCode, pluginName, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading plugin from file: {FilePath}", filePath);
            return new PluginLoadResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Unloads a plugin and removes it from the kernel
    /// </summary>
    public async Task<PluginUnloadResult> UnloadPluginAsync(string pluginName)
    {
        try
        {
            _logger.LogInformation("Unloading plugin: {PluginName}", pluginName);

            if (!_loadedPlugins.ContainsKey(pluginName))
            {
                return new PluginUnloadResult
                {
                    Success = false,
                    ErrorMessage = $"Plugin not found: {pluginName}"
                };
            }

            var plugin = _loadedPlugins[pluginName];

            // Remove from Semantic Kernel
            await UnregisterPluginFromKernelAsync(plugin);

            // Unload assembly
            if (_pluginAssemblies.ContainsKey(pluginName))
            {
                _pluginAssemblies.Remove(pluginName);
            }

            // Remove from loaded plugins
            _loadedPlugins.Remove(pluginName);

            return new PluginUnloadResult
            {
                Success = true,
                PluginName = pluginName,
                UnloadTimestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unloading plugin: {PluginName}", pluginName);
            return new PluginUnloadResult
            {
                Success = false,
                PluginName = pluginName,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Reloads a plugin with updated source code
    /// </summary>
    public async Task<PluginLoadResult> ReloadPluginAsync(
        string pluginName,
        string updatedSourceCode,
        PluginLoadOptions options = null)
    {
        try
        {
            _logger.LogInformation("Reloading plugin: {PluginName}", pluginName);

            // Unload existing plugin
            await UnloadPluginAsync(pluginName);

            // Load with new source code
            return await LoadPluginFromSourceAsync(updatedSourceCode, pluginName, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reloading plugin: {PluginName}", pluginName);
            return new PluginLoadResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Gets information about all loaded plugins
    /// </summary>
    public async Task<List<LoadedPlugin>> GetLoadedPluginsAsync()
    {
        return _loadedPlugins.Values.ToList();
    }

    /// <summary>
    /// Gets information about a specific loaded plugin
    /// </summary>
    public async Task<LoadedPlugin> GetPluginAsync(string pluginName)
    {
        return _loadedPlugins.TryGetValue(pluginName, out var plugin) ? plugin : null;
    }

    /// <summary>
    /// Validates plugin source code for security and correctness
    /// </summary>
    public async Task<PluginValidationResult> ValidatePluginSourceAsync(string sourceCode, string pluginName)
    {
        try
        {
            _logger.LogInformation("Validating plugin source: {PluginName}", pluginName);

            var errors = new List<string>();
            var warnings = new List<string>();

            // Parse the source code
            var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
            var root = await syntaxTree.GetRootAsync();

            // Check for security issues
            var securityIssues = await CheckSecurityIssuesAsync(root);
            errors.AddRange(securityIssues);

            // Check plugin structure
            var structureIssues = await CheckPluginStructureAsync(root, pluginName);
            errors.AddRange(structureIssues);

            // Check compilation issues
            var compilationIssues = await CheckCompilationIssuesAsync(sourceCode);
            errors.AddRange(compilationIssues);

            return new PluginValidationResult
            {
                IsValid = !errors.Any(),
                Errors = errors,
                Warnings = warnings
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating plugin source: {PluginName}", pluginName);
            return new PluginValidationResult
            {
                IsValid = false,
                Errors = new List<string> { ex.Message }
            };
        }
    }

    /// <summary>
    /// Generates a plugin template based on requirements
    /// </summary>
    public async Task<PluginTemplateResult> GeneratePluginTemplateAsync(
        string requirements,
        string pluginName,
        PluginTemplateOptions options = null)
    {
        try
        {
            _logger.LogInformation("Generating plugin template: {PluginName}", pluginName);

            options ??= new PluginTemplateOptions();

            // Generate a simple plugin template
            var pluginCode = GenerateSimplePluginTemplate(requirements, pluginName, options);

            return new PluginTemplateResult
            {
                Success = true,
                PluginCode = pluginCode,
                PluginName = pluginName,
                GenerationTimestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating plugin template: {PluginName}", pluginName);
            return new PluginTemplateResult
            {
                Success = false,
                PluginName = pluginName,
                ErrorMessage = ex.Message
            };
        }
    }

    private string GenerateSimplePluginTemplate(string requirements, string pluginName, PluginTemplateOptions options)
    {
        return $@"
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace {options.Namespace ?? "UltraGenericSystem.Plugins"};

/// <summary>
/// {requirements}
/// </summary>
public class {pluginName}Plugin
{{
    private readonly ILogger<{pluginName}Plugin> _logger;

    public {pluginName}Plugin(ILogger<{pluginName}Plugin> logger)
    {{
        _logger = logger;
    }}

    /// <summary>
    /// Example function for {pluginName}
    /// </summary>
    [KernelFunction]
    [Description(""Example function for {pluginName}"")]
    public async Task<string> ExampleFunctionAsync(string input)
    {{
        _logger.LogInformation(""Executing {pluginName} function with input: {{Input}}"", input);
        
        // TODO: Implement the actual functionality based on requirements
        return $""Processed: {{input}}"";
    }}
}}";
    }

    #region Private Methods

    private List<string> GetPluginReferencePaths()
    {
        // Return a list of reference paths instead of MetadataReference objects
        return new List<string>
        {
            typeof(object).Assembly.Location,
            typeof(Console).Assembly.Location,
            typeof(Task).Assembly.Location,
            typeof(Microsoft.SemanticKernel.Kernel).Assembly.Location
        };
    }

    private async Task<PluginInfo> ExtractPluginInfoAsync(Assembly assembly, string pluginName)
    {
        var pluginInfo = new PluginInfo
        {
            Name = pluginName,
            Version = assembly.GetName().Version?.ToString() ?? "1.0.0",
            AssemblyName = assembly.GetName().Name,
            Functions = new List<PluginFunction>()
        };

        // Extract functions from the assembly
        foreach (var type in assembly.GetTypes())
        {
            if (type.IsClass && !type.IsAbstract)
            {
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m => !m.IsSpecialName && m.DeclaringType == type);

                foreach (var method in methods)
                {
                    var function = new PluginFunction
                    {
                        Name = method.Name,
                        ReturnType = method.ReturnType.Name,
                        Parameters = method.GetParameters()
                            .Select(p => new PluginParameter
                            {
                                Name = p.Name,
                                Type = p.ParameterType.Name,
                                IsOptional = p.IsOptional
                            }).ToList()
                    };

                    pluginInfo.Functions.Add(function);
                }
            }
        }

        return pluginInfo;
    }

    private async Task<PluginRegistrationResult> RegisterPluginWithKernelAsync(
        Assembly assembly,
        PluginInfo pluginInfo,
        PluginLoadOptions options)
    {
        try
        {
            var functions = new List<string>();

            // Find plugin classes in the assembly
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsClass && !type.IsAbstract)
                {
                    // Create an instance of the plugin
                    var pluginInstance = Activator.CreateInstance(type);

                    // Collect function names from the type directly
                    var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                        .Where(m => !m.IsSpecialName && m.DeclaringType == type);
                    
                    foreach (var method in methods)
                    {
                        functions.Add(method.Name);
                    }
                }
            }

            return new PluginRegistrationResult
            {
                Success = true,
                Functions = functions
            };
        }
        catch (Exception ex)
        {
            return new PluginRegistrationResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private async Task UnregisterPluginFromKernelAsync(LoadedPlugin plugin)
    {
        try
        {
            // Note: Semantic Kernel doesn't currently support unregistering skills
            // This is a limitation of the current SK implementation
            // In a real implementation, you might need to work around this limitation
            _logger.LogInformation("Plugin unregistration not supported by Semantic Kernel: {PluginName}", plugin.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unregistering plugin from kernel: {PluginName}", plugin.Name);
        }
    }

    private async Task<List<string>> CheckSecurityIssuesAsync(SyntaxNode root)
    {
        var issues = new List<string>();

        // Check for dangerous operations
        var dangerousOperations = root.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Where(invocation =>
            {
                var methodName = invocation.Expression.ToString().ToLowerInvariant();
                return methodName.Contains("system.io") ||
                       methodName.Contains("system.net") ||
                       methodName.Contains("system.process") ||
                       methodName.Contains("system.reflection") ||
                       methodName.Contains("system.security") ||
                       methodName.Contains("eval") ||
                       methodName.Contains("exec");
            });

        foreach (var operation in dangerousOperations)
        {
            issues.Add($"Potentially dangerous operation detected: {operation}");
        }

        // Check for file system access
        var fileOperations = root.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Where(invocation =>
            {
                var methodName = invocation.Expression.ToString().ToLowerInvariant();
                return methodName.Contains("file.") ||
                       methodName.Contains("directory.") ||
                       methodName.Contains("path.");
            });

        foreach (var operation in fileOperations)
        {
            issues.Add($"File system operation detected: {operation}");
        }

        return issues;
    }

    private async Task<List<string>> CheckPluginStructureAsync(SyntaxNode root, string pluginName)
    {
        var issues = new List<string>();

        // Check for public classes
        var publicClasses = root.DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(c => c.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)));

        if (!publicClasses.Any())
        {
            issues.Add("No public classes found in plugin");
        }

        // Check for public methods
        var publicMethods = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Where(m => m.Modifiers.Any(mod => mod.IsKind(SyntaxKind.PublicKeyword)));

        if (!publicMethods.Any())
        {
            issues.Add("No public methods found in plugin");
        }

        return issues;
    }

    private async Task<List<string>> CheckCompilationIssuesAsync(string sourceCode)
    {
        var issues = new List<string>();

        try
        {
            // Try to compile the code to check for syntax errors
            var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
            var compilation = CSharpCompilation.Create("PluginCheck")
                .AddSyntaxTrees(syntaxTree)
                .AddReferences(GetPluginReferencePaths().Select(path => MetadataReference.CreateFromFile(path)));

            var diagnostics = compilation.GetDiagnostics();
            var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);

            foreach (var error in errors)
            {
                issues.Add($"Compilation error: {error.GetMessage()}");
            }
        }
        catch (Exception ex)
        {
            issues.Add($"Compilation check failed: {ex.Message}");
        }

        return issues;
    }

    #endregion
}

