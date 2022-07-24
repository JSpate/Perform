using Perform.Model.Console;

namespace Perform.SequencedEvents;

public interface ISequencedEvent
{
    Task Invoke(IConsole console, Task? previous);

    internal object[] Params { get; }
}