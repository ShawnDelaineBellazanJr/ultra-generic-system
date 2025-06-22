using UltraGenericSystem.Models.SelfEvolution;

namespace UltraGenericSystem.Services;

/// <summary>
/// Interface for runtime plugin loading and management
/// Enables runtime extensibility through dynamic compilation and assembly loading
/// </summary>
public interface IRuntimePluginLoader
{
    /// <summary>
    /// Loads a plugin from source code at runtime
    /// </summary>
    Task<PluginLoadResult> LoadPluginFromSourceAsync(
        string sourceCode,
        string pluginName,
        PluginLoadOptions options = null);

    /// <summary>
    /// Loads a plugin from a file
    /// </summary>
    Task<PluginLoadResult> LoadPluginFromFileAsync(
        string filePath,
        PluginLoadOptions options = null);

    /// <summary>
    /// Unloads a plugin and removes it from the kernel
    /// </summary>
    Task<PluginUnloadResult> UnloadPluginAsync(string pluginName);

    /// <summary>
    /// Reloads a plugin with updated source code
    /// </summary>
    Task<PluginLoadResult> ReloadPluginAsync(
        string pluginName,
        string updatedSourceCode,
        PluginLoadOptions options = null);

    /// <summary>
    /// Gets information about all loaded plugins
    /// </summary>
    Task<List<LoadedPlugin>> GetLoadedPluginsAsync();

    /// <summary>
    /// Gets information about a specific loaded plugin
    /// </summary>
    Task<LoadedPlugin> GetPluginAsync(string pluginName);

    /// <summary>
    /// Validates plugin source code for security and correctness
    /// </summary>
    Task<PluginValidationResult> ValidatePluginSourceAsync(string sourceCode, string pluginName);

    /// <summary>
    /// Generates a plugin template based on requirements
    /// </summary>
    Task<PluginTemplateResult> GeneratePluginTemplateAsync(
        string requirements,
        string pluginName,
        PluginTemplateOptions options = null);
} 