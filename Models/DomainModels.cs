using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace UltraGenericSystem.Models;

/// <summary>
/// Represents an AI agent in the system
/// </summary>
public class Agent : BaseEntity
{
    /// <summary>
    /// Name of the agent
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Type/category of the agent
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Path to the prompt template file
    /// </summary>
    [StringLength(500)]
    public string PromptTemplatePath { get; set; } = string.Empty;

    /// <summary>
    /// JSON configuration for the agent
    /// </summary>
    public string Configuration { get; set; } = string.Empty;

    /// <summary>
    /// Version of the agent
    /// </summary>
    [StringLength(20)]
    public new string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Whether the agent is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Agent's current status
    /// </summary>
    [StringLength(20)]
    public string Status { get; set; } = "Idle";

    /// <summary>
    /// Last time the agent was active
    /// </summary>
    public DateTime? LastActive { get; set; }

    /// <summary>
    /// Skills associated with this agent
    /// </summary>
    public virtual ICollection<Skill> Skills { get; set; } = new List<Skill>();

    /// <summary>
    /// Threads created by this agent
    /// </summary>
    public virtual ICollection<AgentThread> Threads { get; set; } = new List<AgentThread>();

    /// <summary>
    /// Executions performed by this agent
    /// </summary>
    public virtual ICollection<AgentExecution> Executions { get; set; } = new List<AgentExecution>();
}

/// <summary>
/// Represents a skill that can be used by agents
/// </summary>
public class Skill : BaseEntity
{
    /// <summary>
    /// Name of the skill
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Code implementation of the skill
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Programming language of the skill
    /// </summary>
    [StringLength(20)]
    public string Language { get; set; } = "C#";

    /// <summary>
    /// Dependencies required for this skill
    /// </summary>
    public string[] Dependencies { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Whether the skill has been compiled
    /// </summary>
    public bool IsCompiled { get; set; } = false;

    /// <summary>
    /// When the skill was last compiled
    /// </summary>
    public DateTime? LastCompiled { get; set; }

    /// <summary>
    /// Compilation output/result
    /// </summary>
    public string CompilationOutput { get; set; } = string.Empty;

    /// <summary>
    /// Whether the skill has compilation errors
    /// </summary>
    public bool HasErrors { get; set; } = false;

    /// <summary>
    /// Agents that use this skill
    /// </summary>
    public virtual ICollection<Agent> Agents { get; set; } = new List<Agent>();
}

/// <summary>
/// Represents a conversation thread for an agent
/// </summary>
public class AgentThread : BaseEntity
{
    /// <summary>
    /// ID of the agent that owns this thread
    /// </summary>
    [Required]
    public Guid AgentId { get; set; }

    /// <summary>
    /// Unique thread identifier
    /// </summary>
    [Required]
    [StringLength(100)]
    public string ThreadId { get; set; } = string.Empty;

    /// <summary>
    /// Current context of the thread
    /// </summary>
    public string Context { get; set; } = string.Empty;

    /// <summary>
    /// When the thread was last active
    /// </summary>
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether the thread is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Execution data for the thread
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object> ExecutionData { get; set; } = new();

    /// <summary>
    /// The agent that owns this thread
    /// </summary>
    public virtual Agent Agent { get; set; } = null!;

    /// <summary>
    /// Executions in this thread
    /// </summary>
    public virtual ICollection<AgentExecution> Executions { get; set; } = new List<AgentExecution>();
}

/// <summary>
/// Represents an execution of an agent
/// </summary>
public class AgentExecution : BaseEntity
{
    /// <summary>
    /// ID of the agent that performed this execution
    /// </summary>
    [Required]
    public Guid AgentId { get; set; }

    /// <summary>
    /// ID of the thread this execution belongs to
    /// </summary>
    [Required]
    public Guid ThreadId { get; set; }

    /// <summary>
    /// Input provided to the agent
    /// </summary>
    public string Input { get; set; } = string.Empty;

    /// <summary>
    /// Output produced by the agent
    /// </summary>
    public string Output { get; set; } = string.Empty;

    /// <summary>
    /// When the execution started
    /// </summary>
    public DateTime StartTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the execution ended
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// Status of the execution
    /// </summary>
    [StringLength(20)]
    public string Status { get; set; } = "Running";

    /// <summary>
    /// Error message if execution failed
    /// </summary>
    public string Error { get; set; } = string.Empty;

    /// <summary>
    /// Duration of the execution
    /// </summary>
    public TimeSpan Duration => EndTime.HasValue ? EndTime.Value - StartTime : TimeSpan.Zero;

    /// <summary>
    /// Execution data and context
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object> ExecutionData { get; set; } = new();

    /// <summary>
    /// The agent that performed this execution
    /// </summary>
    public virtual Agent Agent { get; set; } = null!;

    /// <summary>
    /// The thread this execution belongs to
    /// </summary>
    public virtual AgentThread Thread { get; set; } = null!;
}

/// <summary>
/// Represents a knowledge entry in the system
/// </summary>
public class KnowledgeEntry : BaseEntity
{
    /// <summary>
    /// Title of the knowledge entry
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Content of the knowledge entry
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Category of the knowledge entry
    /// </summary>
    [StringLength(100)]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Source of the knowledge
    /// </summary>
    [StringLength(500)]
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// When the knowledge was last updated
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Version of the knowledge entry
    /// </summary>
    public new int Version { get; set; } = 1;

    /// <summary>
    /// Whether the knowledge is published
    /// </summary>
    public bool IsPublished { get; set; } = false;
}

/// <summary>
/// Represents a dynamically generated skill
/// </summary>
public class DynamicSkill : BaseEntity
{
    /// <summary>
    /// Name of the dynamic skill
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Source code of the skill
    /// </summary>
    public string SourceCode { get; set; } = string.Empty;

    /// <summary>
    /// Path to the compiled assembly
    /// </summary>
    [StringLength(500)]
    public string CompiledAssemblyPath { get; set; } = string.Empty;

    /// <summary>
    /// Dependencies required for this skill
    /// </summary>
    public string[] Dependencies { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Whether the skill is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// When the skill was last compiled
    /// </summary>
    public DateTime? LastCompiled { get; set; }

    /// <summary>
    /// Compilation result/output
    /// </summary>
    public string CompilationResult { get; set; } = string.Empty;

    /// <summary>
    /// Whether the skill has compilation errors
    /// </summary>
    public bool HasCompilationErrors { get; set; } = false;
}

/// <summary>
/// Represents a documentation entry
/// </summary>
public class DocumentationEntry : BaseEntity
{
    /// <summary>
    /// Title of the documentation
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Content of the documentation
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Category of the documentation
    /// </summary>
    [StringLength(100)]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Format of the documentation (Markdown, HTML, etc.)
    /// </summary>
    [StringLength(20)]
    public string Format { get; set; } = "Markdown";

    /// <summary>
    /// Version number of the documentation
    /// </summary>
    public new int Version { get; set; } = 1;

    /// <summary>
    /// Whether the documentation is published
    /// </summary>
    public bool IsPublished { get; set; } = false;

    /// <summary>
    /// Author of the documentation
    /// </summary>
    [StringLength(100)]
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// When the documentation was published
    /// </summary>
    public DateTime? PublishedDate { get; set; }
} 