using System.Text.Json.Serialization;

namespace Perform.MidiFootPedal;

public class MidiFootPedalConfig(string device, string? failOverDevice, int longPress, IList<int> controlCodes)
{
    [JsonPropertyName("device")]
    public string Device { get; } = device;

    [JsonPropertyName("failOverDevice")]
    public string? FailOverDevice { get; } = failOverDevice;

    [JsonPropertyName("longPress")]
    public int LongPress { get; } = longPress;

    [JsonPropertyName("controlCodes")]
    public IList<int> ControlCodes { get; } = controlCodes;
}