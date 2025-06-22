using UltraGenericSystem.Models.SelfEvolution;

namespace UltraGenericSystem.Services.SelfEvolution;

/// <summary>
/// Service for dynamic plugin management and registration
/// </summary>
public interface IDynamicPluginService
{
    /// <summary>
    /// Register a new plugin
    /// </summary>
    Task<DynamicPlugin> RegisterPluginAsync(DynamicPlugin plugin);
    
    /// <summary>
    /// Load a plugin into memory
    /// </summary>
    Task<bool> LoadPluginAsync(string pluginName);
    
    /// <summary>
    /// Unload a plugin from memory
    /// </summary>
    Task<bool> UnloadPluginAsync(string pluginName);
    
    /// <summary>
    /// Get all registered plugins
    /// </summary>
    Task<List<DynamicPlugin>> GetPluginsAsync(string? pluginType = null);
    
    /// <summary>
    /// Get a specific plugin
    /// </summary>
    Task<DynamicPlugin?> GetPluginAsync(string pluginName);
    
    /// <summary>
    /// Update plugin configuration
    /// </summary>
    Task<DynamicPlugin> UpdatePluginAsync(DynamicPlugin plugin);
    
    /// <summary>
    /// Delete a plugin
    /// </summary>
    Task<bool> DeletePluginAsync(string pluginName);
    
    /// <summary>
    /// Compile and register a plugin from source code
    /// </summary>
    Task<DynamicPlugin> CompileAndRegisterPluginAsync(string pluginName, string sourceCode, string pluginType);
    
    /// <summary>
    /// Execute a plugin method
    /// </summary>
    Task<object?> ExecutePluginMethodAsync(string pluginName, string methodName, params object[] parameters);
    
    /// <summary>
    /// Get plugin statistics
    /// </summary>
    Task<Dictionary<string, object>> GetPluginStatisticsAsync();
    
    /// <summary>
    /// Validate plugin source code
    /// </summary>
    Task<bool> ValidatePluginSourceAsync(string sourceCode);
    
    /// <summary>
    /// Get plugin dependencies
    /// </summary>
    Task<List<string>> GetPluginDependenciesAsync(string pluginName);
    
    /// <summary>
    /// Check plugin compatibility
    /// </summary>
    Task<bool> CheckPluginCompatibilityAsync(string pluginName);
} 