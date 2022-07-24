using System.Text.Json.Serialization;
using Perform.SequencedEvents;

namespace Perform.Model;

public class EventSequence
{
    [JsonConstructor]
    public EventSequence(string trigger, Sequence sequence)
    {
        Trigger = trigger;
        Sequence = sequence;
    }

    [JsonPropertyName("trigger")]
    public string Trigger { get; }

    [JsonPropertyName("sequence")]
    public Sequence Sequence { get; }
}