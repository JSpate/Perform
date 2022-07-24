namespace Perform.Model.Console;

public interface IMidiConsole : IConsole
{
    void SendMidiNote(int channel, int note, int value);

    void SendMidiControlChange(int channel, int change, int value);
}