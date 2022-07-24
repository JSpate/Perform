using System.Text.Json.Serialization;

namespace Perform.Model;

public class Trigger
{
    [JsonConstructor]
    public Trigger(int trackId, int level)
    {
        TrackId = trackId;
        Level = level;
    }

    [JsonPropertyName("trackId")]
    public int TrackId { get; }

    [JsonPropertyName("level")]
    public int Level { get; }
}