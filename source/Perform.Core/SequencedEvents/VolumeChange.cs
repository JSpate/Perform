using Perform.Model.Console;

namespace Perform.SequencedEvents;

public class VolumeChange : ISequencedEvent
{
    private readonly int _change;
    private readonly int _track;

    public VolumeChange(int track, int change)
    {
        _track = track;
        _change = change;
    }

    public async Task Invoke(IConsole console, Task? previous)
    {
        console.Track(_track).Volume += _change;
        await Task.CompletedTask;
    }

    object[] ISequencedEvent.Params => new object[] { _track, _change };
}