using System.Text.Json.Serialization;

namespace Perform.Model;

public class SongAction
{
    [JsonConstructor]
    public SongAction(SongActionType type, string name, string? trigger)
    {
        Type = type;
        Name = name;
        Trigger = trigger;
    }

    [JsonPropertyName("type")]
    public SongActionType Type { get; }

    [JsonPropertyName("name")]
    public string Name { get; }

    [JsonPropertyName("trigger")]
    public string? Trigger { get; }
}