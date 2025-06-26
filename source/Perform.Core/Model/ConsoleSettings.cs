using System.Dynamic;
using System.Text.Json;

namespace Perform.Model;

public class ConsoleSettings
{
    public string Name { get; set; } = string.Empty;

    public ExpandoObject? Config { get; set; } = null;

    public Dictionary<string, Dictionary<string, JsonElement>> Tracks { get; set; } = new();

    public Dictionary<int, Dictionary<string, Dictionary<string, JsonElement>>> Stomp { get; set; } = new();
}