using Perform.Model.Console;

namespace Perform.SequencedEvents;

public class Sequence
{
    private readonly List<ISequencedEvent> _events;

    public Sequence(params ISequencedEvent[] events)
    {
        _events = events.ToList();
    }

    public Sequence(IEnumerable<ISequencedEvent> events)
    {
        _events = events.ToList();
    }

    public async Task Invoke(IConsole console)
    {
        var task = _events[0].Invoke(console, null);
        foreach (var sequencedEvent in _events.Skip(1))
        {
            async Task ContinuationFunction(Task t) => await sequencedEvent.Invoke(console, t);

            task = await task.ContinueWith(ContinuationFunction);
        }

        await task;
    }
}