using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Spectre.Console;
using System.Text;

namespace UltraGenericSystem.Services;

/// <summary>
/// Service for logging agent conversations in a human-readable format
/// </summary>
public class ConversationalLogger : IConversationalLogger
{
    private readonly ILogger<ConversationalLogger> _logger;
    private readonly Dictionary<string, Color> _agentColors;
    private readonly Dictionary<string, string> _agentIcons;
    private readonly StringBuilder _conversationBuffer;
    private readonly object _lockObject = new object();

    public ConversationalLogger(ILogger<ConversationalLogger> logger)
    {
        _logger = logger;
        _conversationBuffer = new StringBuilder();
        // Enterprise color palette
        _agentColors = new Dictionary<string, Color>
        {
            ["orchestrator"] = Color.Blue,
            ["planner"] = Color.Green,
            ["maker"] = Color.Yellow,
            ["checker"] = Color.Red,
            ["reflector"] = Color.Aqua,
            ["default"] = Color.Grey
        };
        // Minimal icons
        _agentIcons = new Dictionary<string, string>
        {
            ["orchestrator"] = "🤖",
            ["planner"] = "📝",
            ["maker"] = "🔨",
            ["checker"] = "✅",
            ["reflector"] = "💡",
            ["default"] = "🤖"
        };
    }

    private string Timestamp => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    private bool UseAnsi => AnsiConsole.Profile.Capabilities.Ansi;

    /// <summary>
    /// Logs a message from an agent in conversational format
    /// </summary>
    public void LogAgentMessage(string agentName, string agentType, string message, string? thinking = null)
    {
        lock (_lockObject)
        {
            var color = _agentColors.ContainsKey(agentType) ? _agentColors[agentType] : _agentColors["default"];
            var icon = _agentIcons.ContainsKey(agentType) ? _agentIcons[agentType] : _agentIcons["default"];
            var header = $"{icon} [bold {color}]{agentName}[/] [grey]({agentType})[/]   [dim]{Timestamp}[/]";
            
            var footer = !string.IsNullOrEmpty(thinking) ? $"[dim italic]💭 {agentName} thinking: {thinking}[/]" : null;
            
            // Check if message contains problematic content that might cause parsing issues
            bool hasProblematicContent = message.Contains("[PYTHON]") || message.Contains("[JAVASCRIPT]") || 
                                       message.Contains("[CODE]") || message.Contains("[HTML]") ||
                                       message.Contains("[CSS]") || message.Contains("[JSON]");
            
            if (UseAnsi && !hasProblematicContent)
            {
                try
                {
                    // Use plain text for message content to avoid parsing issues
                    var messageContent = !string.IsNullOrEmpty(thinking) 
                        ? $"{message}\n\n[dim italic]{footer}[/]"
                        : message;
                    var panel = new Panel(messageContent)
                        .Header(header)
                        .Border(BoxBorder.Rounded)
                        .BorderColor(color)
                        .Padding(1, 0)
                        .Expand();
                    AnsiConsole.Write(panel);
                    AnsiConsole.WriteLine();
                }
                catch (Exception)
                {
                    // Fall back to plain console output if Spectre.Console fails
                    Console.WriteLine($"[{Timestamp}] {agentName} ({agentType}): {message}");
                    if (!string.IsNullOrEmpty(thinking))
                        Console.WriteLine($"[{Timestamp}] 💭 {agentName} thinking: {thinking}");
                }
            }
            else
            {
                Console.WriteLine($"[{Timestamp}] {agentName} ({agentType}): {message}");
                if (!string.IsNullOrEmpty(thinking))
                    Console.WriteLine($"[{Timestamp}] 💭 {agentName} thinking: {thinking}");
            }
            
            _conversationBuffer.AppendLine($"{Timestamp} {agentName} ({agentType}): {message}");
            if (!string.IsNullOrEmpty(thinking))
                _conversationBuffer.AppendLine($"{Timestamp} 💭 {agentName} thinking: {thinking}");
        }
    }

    /// <summary>
    /// Logs a system message
    /// </summary>
    public void LogSystemMessage(string message)
    {
        LogSystemMessage(message, Color.Grey);
    }

    /// <summary>
    /// Logs a system message with custom color
    /// </summary>
    public void LogSystemMessage(string message, Color color)
    {
        lock (_lockObject)
        {
            var msg = $"[grey]{Timestamp}[/] [{color}]{message}[/]";
            if (UseAnsi)
                AnsiConsole.MarkupLine(msg);
            else
                Console.WriteLine($"{Timestamp} {message}");
            _conversationBuffer.AppendLine($"{Timestamp} {message}");
        }
    }

    /// <summary>
    /// Logs the start of a conversation or task
    /// </summary>
    public void LogConversationStart(string conversationId, string task)
    {
        lock (_lockObject)
        {
            var header = $"[bold blue]CONVERSATION STARTED[/]";
            var content = $"[grey]{Timestamp}[/]\n[white]Task:[/] [bold]{task}[/]";
            if (UseAnsi)
            {
                AnsiConsole.Write(new Panel(content)
                    .Header(header)
                    .Border(BoxBorder.Double)
                    .BorderColor(Color.Blue)
                    .Padding(1, 1));
            }
            else
            {
                Console.WriteLine($"=== CONVERSATION STARTED ===\n{Timestamp}\nTask: {task}\n");
            }
            _conversationBuffer.AppendLine($"CONVERSATION STARTED: {task} @ {Timestamp}");
        }
    }

    /// <summary>
    /// Logs the end of a conversation or task
    /// </summary>
    public void LogConversationEnd(string conversationId, string result)
    {
        lock (_lockObject)
        {
            var footer = $"[bold green]CONVERSATION ENDED[/]";
            var content = $"[grey]{Timestamp}[/]\n[white]Result:[/] [bold]{result}[/]";
            if (UseAnsi)
            {
                AnsiConsole.Write(new Panel(content)
                    .Header(footer)
                    .Border(BoxBorder.Double)
                    .BorderColor(Color.Green)
                    .Padding(1, 1));
            }
            else
            {
                Console.WriteLine($"=== CONVERSATION ENDED ===\n{Timestamp}\nResult: {result}\n");
            }
            _conversationBuffer.AppendLine($"CONVERSATION ENDED: {result} @ {Timestamp}");
        }
    }

    /// <summary>
    /// Logs an error in conversational format
    /// </summary>
    public void LogError(string error)
    {
        lock (_lockObject)
        {
            var content = $"[grey]{Timestamp}[/]\n[white]Error:[/] [bold red]{error}[/]";
            if (UseAnsi)
            {
                AnsiConsole.Write(new Panel(content)
                    .Header("[red]❌ ERROR[/]")
                    .Border(BoxBorder.Double)
                    .BorderColor(Color.Red));
            }
            else
            {
                Console.WriteLine($"{Timestamp} ❌ ERROR: {error}");
            }
            _conversationBuffer.AppendLine($"{Timestamp} ❌ ERROR: {error}");
        }
    }

    /// <summary>
    /// Logs a warning in conversational format
    /// </summary>
    public void LogWarning(string warning)
    {
        lock (_lockObject)
        {
            var content = $"[grey]{Timestamp}[/]\n[white]Warning:[/] [bold yellow]{warning}[/]";
            if (UseAnsi)
            {
                AnsiConsole.Write(new Panel(content)
                    .Header("[yellow]⚠️ WARNING[/]")
                    .Border(BoxBorder.Double)
                    .BorderColor(Color.Yellow));
            }
            else
            {
                Console.WriteLine($"{Timestamp} ⚠️ WARNING: {warning}");
            }
            _conversationBuffer.AppendLine($"{Timestamp} ⚠️ WARNING: {warning}");
        }
    }

    /// <summary>
    /// Logs a success message in conversational format
    /// </summary>
    public void LogSuccess(string success)
    {
        lock (_lockObject)
        {
            var content = $"[grey]{Timestamp}[/]\n[white]Success:[/] [bold green]{success}[/]";
            if (UseAnsi)
            {
                AnsiConsole.Write(new Panel(content)
                    .Header("[green]✅ SUCCESS[/]")
                    .Border(BoxBorder.Double)
                    .BorderColor(Color.Green));
            }
            else
            {
                Console.WriteLine($"{Timestamp} ✅ SUCCESS: {success}");
            }
            _conversationBuffer.AppendLine($"{Timestamp} ✅ SUCCESS: {success}");
        }
    }

    /// <summary>
    /// Logs a decision or action taken by an agent
    /// </summary>
    public void LogAgentDecision(string agentName, string decision, string reasoning)
    {
        lock (_lockObject)
        {
            var content = $"[grey]{Timestamp}[/]\n[white]Decision:[/] [bold]{decision}[/]\n[white]Reasoning:[/] {reasoning}";
            if (UseAnsi)
            {
                AnsiConsole.Write(new Panel(content)
                    .Header($"[yellow]Decision by {agentName}[/]")
                    .Border(BoxBorder.Rounded)
                    .BorderColor(Color.Yellow));
            }
            else
            {
                Console.WriteLine($"{Timestamp} Decision by {agentName}: {decision}\nReasoning: {reasoning}");
            }
            _conversationBuffer.AppendLine($"{Timestamp} {agentName} decided: {decision} (Reasoning: {reasoning})");
        }
    }

    /// <summary>
    /// Logs a thinking process or internal reasoning
    /// </summary>
    public void LogAgentThinking(string agentName, string thinking)
    {
        lock (_lockObject)
        {
            var footer = $"[dim italic]💭 {agentName} thinking: {thinking}[/]";
            if (UseAnsi)
            {
                var panel = new Panel(footer)
                    .Border(BoxBorder.Rounded)
                    .BorderColor(Color.Grey)
                    .Padding(1, 0)
                    .Expand();
                AnsiConsole.Write(panel);
                AnsiConsole.WriteLine();
            }
            else
            {
                Console.WriteLine($"[{Timestamp}] 💭 {agentName} thinking: {thinking}");
            }
            _conversationBuffer.AppendLine($"{Timestamp} 💭 {agentName} thinking: {thinking}");
        }
    }

    /// <summary>
    /// Logs a table of active agents
    /// </summary>
    public void LogAgentTable(IEnumerable<dynamic> agents)
    {
        lock (_lockObject)
        {
            var table = new Table()
                .Title("[bold blue]Team/Agent Status[/]")
                .AddColumn("[bold]Agent Name[/]")
                .AddColumn("[bold]Type[/]")
                .AddColumn("[bold]Status[/]")
                .AddColumn("[bold]Capabilities[/]");
            foreach (var agent in agents)
            {
                var agentType = agent.GetType().GetProperty("Type")?.GetValue(agent)?.ToString() ?? "Unknown";
                var color = _agentColors.ContainsKey(agentType) ? _agentColors[agentType] : _agentColors["default"];
                var icon = _agentIcons.ContainsKey(agentType) ? _agentIcons[agentType] : _agentIcons["default"];
                table.AddRow(
                    $"[{color}]{icon} {agent.GetType().GetProperty("Name")?.GetValue(agent)}[/]",
                    $"[{color}]{agentType}[/]",
                    "[green]Active[/]",
                    "[grey]Planning, Execution, Review[/]"
                );
            }
            table.Border(TableBorder.Rounded);
            if (UseAnsi)
                AnsiConsole.Write(table);
            else
                Console.WriteLine("Team/Agent Status Table (see logs for details)");
        }
    }

    /// <summary>
    /// Logs progress of an operation
    /// </summary>
    public void LogProgress(string operation, int current, int total)
    {
        lock (_lockObject)
        {
            if (UseAnsi)
            {
                AnsiConsole.Progress()
                    .Start(ctx =>
                    {
                        var task = ctx.AddTask(operation, maxValue: total);
                        task.Value = current;
                    });
            }
            else
            {
                Console.WriteLine($"{Timestamp} {operation}: {current}/{total}");
            }
        }
    }

    /// <summary>
    /// Gets the full conversation history
    /// </summary>
    public string GetConversationLog()
    {
        lock (_lockObject)
        {
            return _conversationBuffer.ToString();
        }
    }

    /// <summary>
    /// Clears the conversation buffer
    /// </summary>
    public void ClearConversationLog()
    {
        lock (_lockObject)
        {
            _conversationBuffer.Clear();
        }
    }
} 