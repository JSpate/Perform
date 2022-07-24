using Perform.Model.Console;

namespace Perform.SequencedEvents;

public class SetFxPreset : ISequencedEvent
{
    private readonly int _trackId;
    private readonly int _fxId;
    private readonly string _preset;

    public SetFxPreset(int trackId, int fxId, string preset)
    {
        _trackId = trackId;
        _fxId = fxId;
        _preset = preset;
    }

    public async Task Invoke(IConsole console, Task? previous)
    {
        console.Track(_trackId).Fx(_fxId).Preset = _preset;
        await Task.CompletedTask;
    }

    object[] ISequencedEvent.Params => new object[] { _trackId, _fxId, _preset };
}