namespace UltraGenericSystem.Models;

public class Thought
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Input { get; set; } = string.Empty;
    public string Output { get; set; } = string.Empty;
    public List<string> Dependencies { get; set; } = new List<string>();
}
