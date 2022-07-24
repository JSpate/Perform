using System.Text.Json.Serialization;

namespace Perform.Model;

public class ChaseLight
{
    [JsonConstructor]
    public ChaseLight(
        string name, 
        ChaseProperties? color, 
        ChaseProperties? brightness)
    {
        Name = name;
        Color = color;
        Brightness = brightness;
    }

    [JsonPropertyName("name")]
    public string Name { get; }

    [JsonPropertyName("color")]
    public ChaseProperties? Color { get; }

    [JsonPropertyName("brightness")]
    public ChaseProperties? Brightness { get; }
}