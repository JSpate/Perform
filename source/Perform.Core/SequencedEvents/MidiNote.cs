using Perform.Model.Console;

namespace Perform.SequencedEvents;

public class MidiNote : ISequencedEvent
{
    private readonly int _channel;
    private readonly int _note;
    private readonly int _value;

    public MidiNote(int channel, int note, int value)
    {
        _channel = channel;
        _note = note;
        _value = value;
    }

    public async Task Invoke(IConsole console, Task? previous)
    {
        if (console is IMidiConsole midi)
        {
            midi.SendMidiNote(_channel, _note, _value);
        }

        await Task.CompletedTask;
    }

    object[] ISequencedEvent.Params => new object[] { _channel, _note, _value };
}