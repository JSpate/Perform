using Perform.Model.Console;

namespace Perform.SequencedEvents;

public class Unmute : ISequencedEvent
{
    private readonly int _trackId;
    
    public Unmute(int trackId)
    {
        _trackId = trackId;
    }

    public async Task Invoke(IConsole console, Task? previous)
    {
        console.Track(_trackId).Mute = false;
        await Task.CompletedTask;
    }

    object[] ISequencedEvent.Params => new object[] { _trackId };
}