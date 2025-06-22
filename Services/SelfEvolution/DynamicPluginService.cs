using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using UltraGenericSystem.Models.SelfEvolution;
using UltraGenericSystem.Repositories;

namespace UltraGenericSystem.Services.SelfEvolution;

/// <summary>
/// Service for dynamic plugin management and registration
/// </summary>
public class DynamicPluginService : IDynamicPluginService
{
    private readonly IGenericRepository<DynamicPlugin> _pluginRepository;
    private readonly ICodeGenerationService _codeGenerationService;
    private readonly ILogger<DynamicPluginService> _logger;
    private readonly Dictionary<string, Assembly> _loadedAssemblies;
    private readonly Dictionary<string, object> _pluginInstances;

    public DynamicPluginService(
        IGenericRepository<DynamicPlugin> pluginRepository,
        ICodeGenerationService codeGenerationService,
        ILogger<DynamicPluginService> logger)
    {
        _pluginRepository = pluginRepository;
        _codeGenerationService = codeGenerationService;
        _logger = logger;
        _loadedAssemblies = new Dictionary<string, Assembly>();
        _pluginInstances = new Dictionary<string, object>();
    }

    /// <summary>
    /// Register a new plugin
    /// </summary>
    public async Task<DynamicPlugin> RegisterPluginAsync(DynamicPlugin plugin)
    {
        _logger.LogInformation("Registering plugin: {PluginName}", plugin.Name);

        // Validate plugin
        if (!await ValidatePluginSourceAsync(plugin.SourceCode))
        {
            throw new InvalidOperationException($"Invalid plugin source code for {plugin.Name}");
        }

        // Check if plugin already exists
        var existing = await _pluginRepository.QueryAsync(p => p.Name == plugin.Name);
        var existingPlugin = existing.FirstOrDefault();
        if (existingPlugin != null)
        {
            throw new InvalidOperationException($"Plugin '{plugin.Name}' already exists");
        }

        // Save plugin
        var savedPlugin = await _pluginRepository.CreateAsync(plugin);
        
        _logger.LogInformation("Plugin registered successfully: {PluginName}", plugin.Name);
        return savedPlugin;
    }

    /// <summary>
    /// Load a plugin into memory
    /// </summary>
    public async Task<bool> LoadPluginAsync(string pluginName)
    {
        _logger.LogInformation("Loading plugin: {PluginName}", pluginName);

        try
        {
            var plugins = await _pluginRepository.QueryAsync(p => p.Name == pluginName);
            var plugin = plugins.FirstOrDefault();
            if (plugin == null)
            {
                _logger.LogWarning("Plugin not found: {PluginName}", pluginName);
                return false;
            }

            if (plugin.IsLoaded)
            {
                _logger.LogInformation("Plugin already loaded: {PluginName}", pluginName);
                return true;
            }

            // Compile plugin if needed
            if (plugin.CompiledAssembly == null || plugin.CompiledAssembly.Length == 0)
            {
                var compilationResult = await _codeGenerationService.CompileCodeAsync(
                    plugin.SourceCode, 
                    $"Plugin_{pluginName}");

                if (!compilationResult.Success)
                {
                    _logger.LogError("Failed to compile plugin: {PluginName}", pluginName);
                    return false;
                }

                plugin.CompiledAssembly = compilationResult.AssemblyBytes;
                await _pluginRepository.UpdateAsync(plugin);
            }

            // Load assembly
            using var ms = new MemoryStream(plugin.CompiledAssembly);
            var assembly = Assembly.Load(ms.ToArray());
            _loadedAssemblies[pluginName] = assembly;

            // Create instance
            var instance = CreatePluginInstance(assembly, plugin.PluginType);
            if (instance != null)
            {
                _pluginInstances[pluginName] = instance;
                plugin.IsLoaded = true;
                await _pluginRepository.UpdateAsync(plugin);
                
                _logger.LogInformation("Plugin loaded successfully: {PluginName}", pluginName);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading plugin: {PluginName}", pluginName);
            return false;
        }
    }

    /// <summary>
    /// Unload a plugin from memory
    /// </summary>
    public async Task<bool> UnloadPluginAsync(string pluginName)
    {
        _logger.LogInformation("Unloading plugin: {PluginName}", pluginName);

        try
        {
            var plugins = await _pluginRepository.QueryAsync(p => p.Name == pluginName);
            var plugin = plugins.FirstOrDefault();
            if (plugin == null)
            {
                return false;
            }

            // Remove from memory
            _loadedAssemblies.Remove(pluginName);
            _pluginInstances.Remove(pluginName);

            // Update database
            plugin.IsLoaded = false;
            await _pluginRepository.UpdateAsync(plugin);

            _logger.LogInformation("Plugin unloaded successfully: {PluginName}", pluginName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unloading plugin: {PluginName}", pluginName);
            return false;
        }
    }

    /// <summary>
    /// Get all registered plugins
    /// </summary>
    public async Task<List<DynamicPlugin>> GetPluginsAsync(string? pluginType = null)
    {
        var plugins = await _pluginRepository.GetAllAsync();
        
        if (!string.IsNullOrEmpty(pluginType))
        {
            plugins = plugins.Where(p => p.PluginType == pluginType);
        }

        return plugins.ToList();
    }

    /// <summary>
    /// Get a specific plugin
    /// </summary>
    public async Task<DynamicPlugin?> GetPluginAsync(string pluginName)
    {
        var plugins = await _pluginRepository.QueryAsync(p => p.Name == pluginName);
        return plugins.FirstOrDefault();
    }

    /// <summary>
    /// Update plugin configuration
    /// </summary>
    public async Task<DynamicPlugin> UpdatePluginAsync(DynamicPlugin plugin)
    {
        _logger.LogInformation("Updating plugin: {PluginName}", plugin.Name);

        var existing = await _pluginRepository.QueryAsync(p => p.Name == plugin.Name);
        var existingPlugin = existing.FirstOrDefault();
        if (existingPlugin == null)
        {
            throw new InvalidOperationException($"Plugin '{plugin.Name}' not found");
        }

        // If source code changed, recompile
        if (existingPlugin.SourceCode != plugin.SourceCode)
        {
            plugin.IsLoaded = false;
            plugin.CompiledAssembly = null;
            _loadedAssemblies.Remove(plugin.Name);
            _pluginInstances.Remove(plugin.Name);
        }

        return await _pluginRepository.UpdateAsync(plugin);
    }

    /// <summary>
    /// Delete a plugin
    /// </summary>
    public async Task<bool> DeletePluginAsync(string pluginName)
    {
        _logger.LogInformation("Deleting plugin: {PluginName}", pluginName);

        try
        {
            var plugins = await _pluginRepository.QueryAsync(p => p.Name == pluginName);
            var plugin = plugins.FirstOrDefault();
            if (plugin == null)
            {
                return false;
            }

            // Unload if loaded
            if (plugin.IsLoaded)
            {
                await UnloadPluginAsync(pluginName);
            }

            // Delete from database
            await _pluginRepository.DeleteAsync(plugin.Id);
            
            _logger.LogInformation("Plugin deleted successfully: {PluginName}", pluginName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting plugin: {PluginName}", pluginName);
            return false;
        }
    }

    /// <summary>
    /// Compile and register a plugin from source code
    /// </summary>
    public async Task<DynamicPlugin> CompileAndRegisterPluginAsync(string pluginName, string sourceCode, string pluginType)
    {
        _logger.LogInformation("Compiling and registering plugin: {PluginName}", pluginName);

        // Validate source code
        if (!await ValidatePluginSourceAsync(sourceCode))
        {
            throw new InvalidOperationException($"Invalid plugin source code for {pluginName}");
        }

        // Compile plugin
        var compilationResult = await _codeGenerationService.CompileCodeAsync(sourceCode, $"Plugin_{pluginName}");
        if (!compilationResult.Success)
        {
            throw new InvalidOperationException($"Failed to compile plugin {pluginName}: {string.Join(", ", compilationResult.Diagnostics.Select(d => d.Message))}");
        }

        // Create plugin
        var plugin = new DynamicPlugin
        {
            Name = pluginName,
            PluginType = pluginType,
            SourceCode = sourceCode,
            CompiledAssembly = compilationResult.AssemblyBytes,
            IsActive = true,
            IsLoaded = false,
            Description = $"Dynamically compiled {pluginType} plugin",
            Version = "1.0.0"
        };

        // Register plugin
        return await RegisterPluginAsync(plugin);
    }

    /// <summary>
    /// Execute a plugin method
    /// </summary>
    public async Task<object?> ExecutePluginMethodAsync(string pluginName, string methodName, params object[] parameters)
    {
        _logger.LogInformation("Executing plugin method: {PluginName}.{MethodName}", pluginName, methodName);

        try
        {
            // Ensure plugin is loaded
            if (!_pluginInstances.ContainsKey(pluginName))
            {
                var loaded = await LoadPluginAsync(pluginName);
                if (!loaded)
                {
                    throw new InvalidOperationException($"Failed to load plugin: {pluginName}");
                }
            }

            var instance = _pluginInstances[pluginName];
            var method = instance.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);

            if (method == null)
            {
                throw new InvalidOperationException($"Method '{methodName}' not found in plugin '{pluginName}'");
            }

            // Execute method
            var result = method.Invoke(instance, parameters);
            
            // Handle async methods
            if (result is Task task)
            {
                await task;
                var resultProperty = task.GetType().GetProperty("Result");
                return resultProperty?.GetValue(task);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing plugin method: {PluginName}.{MethodName}", pluginName, methodName);
            throw;
        }
    }

    /// <summary>
    /// Get plugin statistics
    /// </summary>
    public async Task<Dictionary<string, object>> GetPluginStatisticsAsync()
    {
        var plugins = await _pluginRepository.GetAllAsync();

        return new Dictionary<string, object>
        {
            ["TotalPlugins"] = plugins.Count(),
            ["ActivePlugins"] = plugins.Count(p => p.IsActive),
            ["LoadedPlugins"] = plugins.Count(p => p.IsLoaded),
            ["PluginTypes"] = plugins.Select(p => p.PluginType).Distinct().ToList(),
            ["LoadedAssemblies"] = _loadedAssemblies.Count,
            ["PluginInstances"] = _pluginInstances.Count
        };
    }

    /// <summary>
    /// Validate plugin source code
    /// </summary>
    public async Task<bool> ValidatePluginSourceAsync(string sourceCode)
    {
        try
        {
            return await _codeGenerationService.ValidateCodeAsync(sourceCode);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Get plugin dependencies
    /// </summary>
    public async Task<List<string>> GetPluginDependenciesAsync(string pluginName)
    {
        var plugins = await _pluginRepository.QueryAsync(p => p.Name == pluginName);
        var plugin = plugins.FirstOrDefault();
        return plugin?.Dependencies ?? new List<string>();
    }

    /// <summary>
    /// Check plugin compatibility
    /// </summary>
    public async Task<bool> CheckPluginCompatibilityAsync(string pluginName)
    {
        try
        {
            var plugins = await _pluginRepository.QueryAsync(p => p.Name == pluginName);
            var plugin = plugins.FirstOrDefault();
            if (plugin == null)
            {
                return false;
            }

            // Validate source code
            if (!await ValidatePluginSourceAsync(plugin.SourceCode))
            {
                return false;
            }

            // Try to compile
            var compilationResult = await _codeGenerationService.CompileCodeAsync(
                plugin.SourceCode, 
                $"Test_{pluginName}");

            return compilationResult.Success;
        }
        catch
        {
            return false;
        }
    }

    #region Private Methods

    private object? CreatePluginInstance(Assembly assembly, string pluginType)
    {
        try
        {
            // Find the main class in the assembly
            var types = assembly.GetTypes();
            var mainType = types.FirstOrDefault(t => 
                t.IsClass && 
                !t.IsAbstract && 
                t.IsPublic &&
                (t.Name.EndsWith("Plugin") || t.Name.EndsWith("Skill") || t.Name.EndsWith("Function")));

            if (mainType == null)
            {
                mainType = types.FirstOrDefault(t => t.IsClass && !t.IsAbstract && t.IsPublic);
            }

            if (mainType == null)
            {
                _logger.LogWarning("No suitable class found in plugin assembly");
                return null;
            }

            // Create instance
            var instance = Activator.CreateInstance(mainType);
            return instance;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating plugin instance");
            return null;
        }
    }

    #endregion
} 