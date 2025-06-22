using Spectre.Console;

namespace UltraGenericSystem.Services;

/// <summary>
/// Interface for logging agent conversations in a human-readable format
/// </summary>
public interface IConversationalLogger
{
    /// <summary>
    /// Logs the start of a conversation
    /// </summary>
    void LogConversationStart(string conversationId, string task);

    /// <summary>
    /// Logs the end of a conversation
    /// </summary>
    void LogConversationEnd(string conversationId, string result);

    /// <summary>
    /// Logs a message from an agent
    /// </summary>
    void LogAgentMessage(string agentName, string agentType, string message, string? thinking = null);

    /// <summary>
    /// Logs a decision or action taken by an agent
    /// </summary>
    void LogAgentDecision(string agentName, string decision, string reasoning);

    /// <summary>
    /// Logs a thinking process or internal reasoning
    /// </summary>
    void LogAgentThinking(string agentName, string thinking);

    /// <summary>
    /// Logs a system message
    /// </summary>
    void LogSystemMessage(string message);

    /// <summary>
    /// Logs a system message with custom color
    /// </summary>
    void LogSystemMessage(string message, Color color);

    /// <summary>
    /// Logs an error
    /// </summary>
    void LogError(string error);

    /// <summary>
    /// Logs a warning
    /// </summary>
    void LogWarning(string warning);

    /// <summary>
    /// Logs a success message
    /// </summary>
    void LogSuccess(string success);

    /// <summary>
    /// Logs a table of active agents
    /// </summary>
    void LogAgentTable(IEnumerable<dynamic> agents);

    /// <summary>
    /// Logs progress of an operation
    /// </summary>
    void LogProgress(string operation, int current, int total);

    /// <summary>
    /// Gets the full conversation log
    /// </summary>
    string GetConversationLog();

    /// <summary>
    /// Clears the conversation log
    /// </summary>
    void ClearConversationLog();
} 