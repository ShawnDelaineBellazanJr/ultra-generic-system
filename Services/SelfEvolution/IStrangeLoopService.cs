using UltraGenericSystem.Models.SelfEvolution;

namespace UltraGenericSystem.Services.SelfEvolution;

/// <summary>
/// Service that implements the strange loop of self-evolution
/// where agents can modify their own workflows and create new capabilities
/// </summary>
public interface IStrangeLoopService
{
    /// <summary>
    /// Execute the strange loop: agent modifies itself
    /// </summary>
    Task<StrangeLoopResult> ExecuteStrangeLoopAsync(StrangeLoopRequest request);

    /// <summary>
    /// Get evolution history for an agent
    /// </summary>
    Task<List<StrangeLoopResult>> GetEvolutionHistoryAsync(string agentId);

    /// <summary>
    /// Check if agent is ready for evolution
    /// </summary>
    Task<bool> IsAgentReadyForEvolutionAsync(string agentId);

    /// <summary>
    /// Get evolution statistics
    /// </summary>
    Task<EvolutionStatistics> GetEvolutionStatisticsAsync();
} 