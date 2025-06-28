using Perform.Interfaces;
using System.Text.Json;

namespace Perform.Model.Console;

public interface IConsole : IDevice
{
    event ValueChangedEventHandler? ValueChanged;

    int? Tempo { get; set; }

    float MasterVolume { get; }

    float MasterPan { get; }

    bool TryGetTrack(int id, out ITrack track);

    bool TryGetTrack(string name, out ITrack track);

    bool? GetState(Dictionary<string, Dictionary<string, JsonElement>> state);

    void SetState(Dictionary<string, Dictionary<string, JsonElement>> state);
}