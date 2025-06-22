using UltraGenericSystem.Models.SelfEvolution;

namespace UltraGenericSystem.Services;

/// <summary>
/// Interface for meta-agent self-evolution capabilities
/// Enables the system to analyze itself, generate improvements, and apply changes safely
/// </summary>
public interface IMetaAgentSelfEvolution
{
    /// <summary>
    /// Analyzes the current system performance and identifies improvement opportunities
    /// </summary>
    Task<SelfEvolutionResult> AnalyzeSystemPerformanceAsync();

    /// <summary>
    /// Generates specific improvements based on analysis results
    /// </summary>
    Task<SelfEvolutionResult> GenerateImprovementsAsync(SystemAnalysis analysis);

    /// <summary>
    /// Applies changes safely with rollback capabilities
    /// </summary>
    Task<SelfEvolutionResult> ApplyChangesAsync(List<ImprovementStrategy> strategies);

    /// <summary>
    /// Validates changes and ensures they meet quality and safety standards
    /// </summary>
    Task<SelfEvolutionResult> ValidateChangesAsync(List<AppliedChange> changes);

    /// <summary>
    /// Executes a complete self-evolution cycle
    /// </summary>
    Task<SelfEvolutionResult> ExecuteEvolutionCycleAsync();
} 