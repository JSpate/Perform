using Perform.Model;
using Perform.Model.Console;

namespace Perform.SequencedEvents;

public class SetLight : ISequencedEvent
{
    private readonly string _name;
    private readonly LightSettings _from;
    private readonly LightSettings _to;
    private readonly int _fadeIn;
    private readonly int _fadeOut;

    public SetLight(string name, string from, string to, int fadeIn, int fadeOut)
    {
        _name = name;
        _from = from;
        _to = to;
        _fadeIn = fadeIn;
        _fadeOut = fadeOut;
    }

    public async Task Invoke(IConsole console, Task? previous)
    {
        await Task.CompletedTask;
    }

    object[] ISequencedEvent.Params => new object[] { _name, (string)_from, (string)_to, _fadeIn, _fadeOut };
}