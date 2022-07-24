using System.Text.Json.Serialization;
using Perform.SequencedEvents;

namespace Perform.Model;

public class SongSection
{
    [JsonConstructor]
    public SongSection(
        Sequence? onEntry, 
        Sequence? onExit,
        IList<ButtonAction> buttons, 
        SongAction action)
    {
        Buttons = buttons;
        Action = action;
        OnEntry = onEntry ?? new Sequence();
        OnExit = onExit ?? new Sequence();
    }

    [JsonPropertyName("onEntry")]
    public Sequence OnEntry { get; }

    [JsonPropertyName("onExit")]
    public Sequence OnExit { get; }

    [JsonPropertyName("action")]
    public SongAction Action { get; }

    [JsonPropertyName("buttons")]
    public IList<ButtonAction> Buttons { get; }
}