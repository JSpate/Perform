using System.Text.Json.Serialization;

namespace Perform.Model;

public class ButtonAction
{
    [JsonConstructor]
    public ButtonAction(int button, ButtonPressType type, string section)
    {
        Section = section;
        Type = type;
        Button = button;
    }

    [JsonPropertyName("button")]
    public int Button { get; }

    [JsonPropertyName("type")]
    public ButtonPressType Type { get; }

    [JsonPropertyName("section")]
    public string Section { get; }
}