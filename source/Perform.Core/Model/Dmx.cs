using System.Text.Json.Serialization;

namespace Perform.Model;

[method: JsonConstructor]
public class Dmx(string name, string type, Dictionary<int, Dictionary<string, int>> settings)
{
    public string Name { get; set; } = name;

    public string Type { get; set; } = type;

    public Dictionary<int, Dictionary<string, int>> Settings { get; set; } = settings;
}