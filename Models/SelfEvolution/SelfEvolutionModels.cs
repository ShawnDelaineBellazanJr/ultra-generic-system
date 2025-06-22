using System.ComponentModel.DataAnnotations;

namespace UltraGenericSystem.Models.SelfEvolution;

/// <summary>
/// Result of self-evolution operations
/// </summary>
public class SelfEvolutionResult
{
    public bool Success { get; set; }
    
    public string? ErrorMessage { get; set; }
    
    public SystemAnalysis? Analysis { get; set; }
    
    public List<ImprovementOpportunity> ImprovementOpportunities { get; set; } = new();
    
    public List<ImprovementStrategy> ImprovementStrategies { get; set; } = new();
    
    public List<AppliedChange> AppliedChanges { get; set; } = new();
    
    public ValidationResult? ValidationResult { get; set; }
    
    public RollbackPlan? RollbackPlan { get; set; }
    
    public ImpactEstimate? EstimatedImpact { get; set; }
    
    public ImplementationPlan? ImplementationPlan { get; set; }
    
    public List<Recommendation> Recommendations { get; set; } = new();
    
    public EvolutionMetrics? EvolutionMetrics { get; set; }
}

/// <summary>
/// System analysis results
/// </summary>
public class SystemAnalysis
{
    public DateTime Timestamp { get; set; }
    
    public PerformanceMetrics PerformanceMetrics { get; set; } = new();
    
    public CodeQualityMetrics CodeQualityMetrics { get; set; } = new();
    
    public ArchitectureMetrics ArchitectureMetrics { get; set; } = new();
    
    public UserFeedbackMetrics UserFeedbackMetrics { get; set; } = new();
    
    public ResourceUtilization ResourceUtilization { get; set; } = new();
    
    public List<ImprovementOpportunity> ImprovementOpportunities { get; set; } = new();
}

/// <summary>
/// Performance metrics for system analysis
/// </summary>
public class PerformanceMetrics
{
    public double ResponseTime { get; set; } // ms
    public double Throughput { get; set; } // requests/sec
    public double ErrorRate { get; set; } // percentage
    public ResourceUsage ResourceUsage { get; set; } = new();
    public double UserSatisfaction { get; set; } // percentage
}

/// <summary>
/// Code quality metrics for analysis
/// </summary>
public class CodeQualityMetrics
{
    public double CyclomaticComplexity { get; set; }
    public double CodeCoverage { get; set; } // percentage
    public double TechnicalDebt { get; set; } // percentage
    public double CodeDuplication { get; set; } // percentage
    public int SecurityVulnerabilities { get; set; }
}

/// <summary>
/// Architecture metrics for analysis
/// </summary>
public class ArchitectureMetrics
{
    public double Coupling { get; set; } // percentage
    public double Cohesion { get; set; } // percentage
    public double Modularity { get; set; } // percentage
    public double Scalability { get; set; } // percentage
    public double Maintainability { get; set; } // percentage
}

/// <summary>
/// User feedback metrics
/// </summary>
public class UserFeedbackMetrics
{
    public double SatisfactionScore { get; set; } // 0-5 scale
    
    public double UsabilityScore { get; set; } // 0-5 scale
    
    public int NumberOfComplaints { get; set; }
    
    public int NumberOfFeatureRequests { get; set; }
    
    public double AdoptionRate { get; set; } // percentage
}

/// <summary>
/// Resource utilization metrics
/// </summary>
public class ResourceUtilization
{
    public double MemoryUtilization { get; set; } // percentage
    
    public double CpuUtilization { get; set; } // percentage
    
    public double DiskUtilization { get; set; } // percentage
    
    public double NetworkUtilization { get; set; } // percentage
    
    public int ActiveConnections { get; set; }
    
    public double DatabaseUtilization { get; set; } // percentage
}

/// <summary>
/// Resource usage metrics
/// </summary>
public class ResourceUsage
{
    public double MemoryUsage { get; set; } // MB
    public double CpuUsage { get; set; } // percentage
    public double DiskUsage { get; set; } // percentage
    public double NetworkUsage { get; set; } // percentage
    public int ActiveConnections { get; set; }
    public double DatabaseUsage { get; set; } // percentage
}

/// <summary>
/// Improvement strategy
/// </summary>
public class ImprovementStrategy
{
    public string Name { get; set; } = string.Empty;
    
    public ImprovementType Type { get; set; }
    
    public string Description { get; set; } = string.Empty;
    
    public Priority Priority { get; set; }
    
    public Impact Impact { get; set; }
    
    public Effort Effort { get; set; }
    
    public List<string> ImplementationSteps { get; set; } = new();
    
    public Dictionary<string, object> Parameters { get; set; } = new();
    
    public double EstimatedCost { get; set; }
    
    public TimeSpan EstimatedDuration { get; set; }
}

/// <summary>
/// Applied change
/// </summary>
public class AppliedChange
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public string StrategyName { get; set; } = string.Empty;
    
    public bool Success { get; set; }
    
    public string? ErrorMessage { get; set; }
    
    public DateTime AppliedAt { get; set; }
    
    public List<string> Changes { get; set; } = new();
    
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Validation result
/// </summary>
public class ValidationResult
{
    public Guid ChangeId { get; set; }
    
    public bool IsValid { get; set; }
    
    public List<QualityCheck> QualityChecks { get; set; } = new();
    
    public List<SafetyCheck> SafetyChecks { get; set; } = new();
    
    public List<PerformanceCheck> PerformanceChecks { get; set; } = new();
    
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Overall validation result
/// </summary>
public class OverallValidationResult
{
    public bool IsValid { get; set; }
    
    public List<ValidationResult> ValidationResults { get; set; } = new();
    
    public double QualityScore { get; set; }
    
    public double SafetyScore { get; set; }
    
    public double PerformanceScore { get; set; }
}

/// <summary>
/// Quality check
/// </summary>
public class QualityCheck
{
    public string Name { get; set; } = string.Empty;
    
    public bool Passed { get; set; }
    
    public string? Details { get; set; }
}

/// <summary>
/// Safety check
/// </summary>
public class SafetyCheck
{
    public string Name { get; set; } = string.Empty;
    
    public bool Passed { get; set; }
    
    public string? Details { get; set; }
}

/// <summary>
/// Performance check
/// </summary>
public class PerformanceCheck
{
    public string Name { get; set; } = string.Empty;
    
    public bool Passed { get; set; }
    
    public string? Details { get; set; }
}

/// <summary>
/// Rollback plan
/// </summary>
public class RollbackPlan
{
    public List<RollbackStep> Steps { get; set; } = new();
    
    public bool CanRollback { get; set; } = true;
    
    public string? RollbackReason { get; set; }
    
    public void AddRollbackStep(AppliedChange change)
    {
        Steps.Add(new RollbackStep
        {
            ChangeId = change.Id,
            StrategyName = change.StrategyName,
            RollbackAction = $"Revert changes for {change.StrategyName}"
        });
    }
}

/// <summary>
/// Rollback step
/// </summary>
public class RollbackStep
{
    public Guid ChangeId { get; set; }
    
    public string StrategyName { get; set; } = string.Empty;
    
    public string RollbackAction { get; set; } = string.Empty;
    
    public bool Executed { get; set; }
    
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Impact estimate for changes
/// </summary>
public class ImpactEstimate
{
    public double PerformanceImpact { get; set; } // percentage
    public double CostImpact { get; set; } // dollars
    public double TimeImpact { get; set; } // hours
    public double RiskLevel { get; set; } // 0-1 scale
    public string? Description { get; set; }
    
    // Additional properties for MetaAgentSelfEvolution compatibility
    public double PerformanceImprovement { get; set; } // percentage
    public double QualityImprovement { get; set; } // percentage
    public double MaintainabilityImprovement { get; set; } // percentage
}

/// <summary>
/// Implementation plan
/// </summary>
public class ImplementationPlan
{
    public List<ImplementationStep> Steps { get; set; } = new();
    
    public TimeSpan TotalDuration { get; set; }
    
    public double TotalCost { get; set; }
    
    public List<string> Dependencies { get; set; } = new();
    
    public List<string> Risks { get; set; } = new();
}

/// <summary>
/// Implementation step
/// </summary>
public class ImplementationStep
{
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public TimeSpan Duration { get; set; }
    
    public double Cost { get; set; }
    
    public List<string> Dependencies { get; set; } = new();
    
    public bool IsCompleted { get; set; }
}

/// <summary>
/// Recommendation
/// </summary>
public class Recommendation
{
    public string Title { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public Priority Priority { get; set; }
    
    public Impact Impact { get; set; }
    
    public Effort Effort { get; set; }
    
    public string? Rationale { get; set; }
    
    public List<string> ImplementationSteps { get; set; } = new();
    
    public string EstimatedEffort { get; set; } = string.Empty;
}

/// <summary>
/// Evolution metrics
/// </summary>
public class EvolutionMetrics
{
    public double ImprovementRate { get; set; } // percentage
    
    public double SuccessRate { get; set; } // percentage
    
    public TimeSpan TimeToImprovement { get; set; }
    
    public double CostSavings { get; set; } // dollars
    
    public int NumberOfEvolutions { get; set; }
    
    public double AverageEvolutionTime { get; set; } // minutes
}

/// <summary>
/// Improvement type enum
/// </summary>
public enum ImprovementType
{
    Performance,
    CodeQuality,
    Architecture,
    Security,
    Usability,
    Maintainability
}

/// <summary>
/// Priority enum
/// </summary>
public enum Priority
{
    Low,
    Medium,
    High,
    Critical
}

/// <summary>
/// Impact enum
/// </summary>
public enum Impact
{
    Low,
    Medium,
    High
}

/// <summary>
/// Effort enum
/// </summary>
public enum Effort
{
    Low,
    Medium,
    High
} 