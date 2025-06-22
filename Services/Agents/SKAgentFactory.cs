using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Orchestration;
using Microsoft.SemanticKernel.Agents.Orchestration.Sequential;
using Microsoft.SemanticKernel.Agents.Orchestration.Concurrent;
using Microsoft.SemanticKernel.Agents.Orchestration.GroupChat;
using Microsoft.SemanticKernel.Agents.Orchestration.Handoff;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;
using Microsoft.SemanticKernel.ChatCompletion;
using UltraGenericSystem.Models;

namespace UltraGenericSystem.Services.Agents;

/// <summary>
/// Response callback delegate for monitoring agent responses
/// </summary>
public delegate ValueTask ResponseCallback(ChatMessageContent response);

/// <summary>
/// Interactive callback delegate for human-in-the-loop scenarios
/// </summary>
public delegate ValueTask<ChatMessageContent> InteractiveCallback();

/// <summary>
/// Factory for creating SK agents using the official Agent Framework
/// </summary>
public class SKAgentFactory
{
    private readonly Kernel _kernel;
    private readonly ILogger<SKAgentFactory> _logger;

    public SKAgentFactory(Kernel kernel, ILogger<SKAgentFactory> logger)
    {
        _kernel = kernel;
        _logger = logger;
    }

    /// <summary>
    /// Creates a ChatCompletionAgent with basic configuration
    /// </summary>
    public ChatCompletionAgent CreateChatCompletionAgent(
        string name,
        string instructions,
        Dictionary<string, object>? arguments = null)
    {
        _logger.LogInformation("Creating ChatCompletionAgent: {Name}", name);

        var agent = new ChatCompletionAgent
        {
            Name = name,
            Instructions = instructions,
            Kernel = _kernel,
            Arguments = arguments != null ? new KernelArguments(arguments) : new KernelArguments()
        };

        return agent;
    }

    /// <summary>
    /// Creates basic orchestration patterns
    /// </summary>
    public OrchestrationPatterns CreateOrchestrationPatterns(IEnumerable<ChatCompletionAgent> agents)
    {
        var agentList = agents.ToList();
        
        return new OrchestrationPatterns
        {
            Sequential = CreateSequentialOrchestration(agentList),
            Concurrent = CreateConcurrentOrchestration(agentList),
            GroupChat = CreateGroupChatOrchestration(agentList),
            Handoff = CreateHandoffOrchestration(agentList)
        };
    }

    private SequentialOrchestration CreateSequentialOrchestration(List<ChatCompletionAgent> agents)
    {
        return new SequentialOrchestration(agents.ToArray());
    }

    private ConcurrentOrchestration CreateConcurrentOrchestration(List<ChatCompletionAgent> agents)
    {
        return new ConcurrentOrchestration(agents.ToArray());
    }

    private GroupChatOrchestration CreateGroupChatOrchestration(List<ChatCompletionAgent> agents)
    {
        // GroupChatOrchestration requires a GroupChatManager
        var manager = new RoundRobinGroupChatManager { MaximumInvocationCount = 10 };
        return new GroupChatOrchestration(manager, agents.ToArray());
    }

    private HandoffOrchestration CreateHandoffOrchestration(List<ChatCompletionAgent> agents)
    {
        // HandoffOrchestration requires OrchestrationHandoffs
        var handoffs = OrchestrationHandoffs
            .StartWith(agents[0])
            .Add(agents[0], agents[1], "Transfer to next agent")
            .Add(agents[1], agents[2], "Transfer to next agent")
            .Add(agents[2], agents[3], "Transfer to next agent")
            .Add(agents[3], agents[4], "Transfer to next agent");

        return new HandoffOrchestration(handoffs, agents.ToArray());
    }

    /// <summary>
    /// Creates all core agents for the system
    /// </summary>
    public List<ChatCompletionAgent> CreateAllAgents()
    {
        var orchestrator = CreateChatCompletionAgent(
            "OrchestratorAgent",
            "You are the orchestrator agent responsible for coordinating and managing the overall workflow.",
            new Dictionary<string, object> { { "role", "orchestrator" } });

        var planner = CreateChatCompletionAgent(
            "PlannerAgent",
            "You are the planner agent responsible for creating detailed plans and strategies.",
            new Dictionary<string, object> { { "role", "planner" } });

        var maker = CreateChatCompletionAgent(
            "MakerAgent",
            "You are the maker agent responsible for executing actions and creating outputs.",
            new Dictionary<string, object> { { "role", "maker" } });

        var checker = CreateChatCompletionAgent(
            "CheckerAgent",
            "You are the checker agent responsible for validation and quality assurance.",
            new Dictionary<string, object> { { "role", "checker" } });

        var reflector = CreateChatCompletionAgent(
            "ReflectorAgent",
            "You are the reflector agent responsible for analysis and improvement suggestions.",
            new Dictionary<string, object> { { "role", "reflector" } });

        return new List<ChatCompletionAgent> { orchestrator, planner, maker, checker, reflector };
    }
}

/// <summary>
/// Container for orchestration patterns
/// </summary>
public class OrchestrationPatterns
{
    public SequentialOrchestration Sequential { get; set; } = null!;
    public ConcurrentOrchestration Concurrent { get; set; } = null!;
    public GroupChatOrchestration GroupChat { get; set; } = null!;
    public HandoffOrchestration Handoff { get; set; } = null!;
} 