using Perform.Model.Console;

namespace Perform.SequencedEvents;

public class Mute : ISequencedEvent
{
    private readonly int _trackId;
    
    public Mute(int trackId)
    {
        _trackId = trackId;
    }

    public Task Invoke(IConsole console, Task? previous)
    {
        console.Track(_trackId).Mute = true;
        return Task.CompletedTask;
    }

    object[] ISequencedEvent.Params => new object[] { _trackId };
}