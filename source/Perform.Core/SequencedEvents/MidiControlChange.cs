using Perform.Model.Console;

namespace Perform.SequencedEvents;

public class MidiControlChange : ISequencedEvent
{
    private readonly int _channel;
    private readonly int _change;
    private readonly int _value;

    public MidiControlChange(int channel, int change, int value)
    {
        _channel = channel;
        _change = change;
        _value = value;
    }

    public async Task Invoke(IConsole console, Task? previous)
    {
        if (console is IMidiConsole midi)
        {
            midi.SendMidiControlChange(_channel, _change, _value);
        }

        await Task.CompletedTask;
    }

    object[] ISequencedEvent.Params => new object[] { _channel, _change, _value };
}