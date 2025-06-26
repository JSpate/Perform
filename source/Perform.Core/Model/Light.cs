namespace Perform.Model;

public class Light
{
    public string? Action { get; set; }

    public Dictionary<int, Dictionary<string, int>> Settings { get; set; } = new();
}