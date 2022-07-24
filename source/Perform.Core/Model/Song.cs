using System.Text.Json.Serialization;
using Perform.SequencedEvents;

namespace Perform.Model;

public class Song
{
    [JsonConstructor]
    public Song(
        string name,
        IDictionary<string, Sequence> sequences,
        IDictionary<string, IList<ChaseLight>> chases,
        Sequence? initializationSequence,
        IDictionary<string, SongSection>? sections)
    {
        Name = name;
        Sequences = sequences;
        Chases = chases;
        InitializationSequence = initializationSequence ?? new Sequence();
        Sections = sections ?? new Dictionary<string, SongSection>();
    }

    [JsonPropertyName("name")]
    public string Name { get; }

    [JsonPropertyName("sequences")]
    public IDictionary<string, Sequence> Sequences { get; }

    [JsonPropertyName("chases")]
    public IDictionary<string, IList<ChaseLight>> Chases { get; }

    [JsonPropertyName("initSequence")]
    public Sequence InitializationSequence { get; }

    [JsonPropertyName("sections")]
    public IDictionary<string, SongSection> Sections { get; }
}