using UltraGenericSystem.Models.SelfEvolution;

namespace UltraGenericSystem.Services.SelfEvolution;

/// <summary>
/// Service for code generation and dynamic compilation
/// </summary>
public interface ICodeGenerationService
{
    /// <summary>
    /// Generate code from a template with parameters
    /// </summary>
    Task<CodeGenerationResult> GenerateCodeAsync(CodeGenerationRequest request);
    
    /// <summary>
    /// Compile source code to assembly
    /// </summary>
    Task<CompilationResult> CompileCodeAsync(string sourceCode, string assemblyName, List<string>? references = null);
    
    /// <summary>
    /// Analyze code for metrics, issues, and suggestions
    /// </summary>
    Task<CodeAnalysisResult> AnalyzeCodeAsync(string sourceCode, string? filePath = null);
    
    /// <summary>
    /// Get available code templates
    /// </summary>
    Task<List<CodeTemplate>> GetTemplatesAsync(string? templateType = null);
    
    /// <summary>
    /// Create or update a code template
    /// </summary>
    Task<CodeTemplate> SaveTemplateAsync(CodeTemplate template);
    
    /// <summary>
    /// Delete a code template
    /// </summary>
    Task<bool> DeleteTemplateAsync(string templateName);
    
    /// <summary>
    /// Generate entity class from schema
    /// </summary>
    Task<CodeGenerationResult> GenerateEntityAsync(string entityName, Dictionary<string, string> properties);
    
    /// <summary>
    /// Generate controller for an entity
    /// </summary>
    Task<CodeGenerationResult> GenerateControllerAsync(string entityName, string entityType);
    
    /// <summary>
    /// Generate service for an entity
    /// </summary>
    Task<CodeGenerationResult> GenerateServiceAsync(string entityName, string entityType);
    
    /// <summary>
    /// Generate repository for an entity
    /// </summary>
    Task<CodeGenerationResult> GenerateRepositoryAsync(string entityName, string entityType);
    
    /// <summary>
    /// Generate complete CRUD stack for an entity
    /// </summary>
    Task<List<CodeGenerationResult>> GenerateCrudStackAsync(string entityName, Dictionary<string, string> properties);
    
    /// <summary>
    /// Validate generated code
    /// </summary>
    Task<bool> ValidateCodeAsync(string sourceCode);
    
    /// <summary>
    /// Get code generation statistics
    /// </summary>
    Task<Dictionary<string, object>> GetStatisticsAsync();
} 