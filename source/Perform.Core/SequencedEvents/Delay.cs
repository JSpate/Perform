using Perform.Model.Console;

namespace Perform.SequencedEvents;

public class Delay : ISequencedEvent
{
    private readonly int _milliseconds;

    public Delay(int milliseconds)
    {
        _milliseconds = milliseconds;
    }

    public async Task Invoke(IConsole console, Task? previous)
    {
        await Task.Delay(_milliseconds);
    }

    object[] ISequencedEvent.Params => new object[] { _milliseconds };
}