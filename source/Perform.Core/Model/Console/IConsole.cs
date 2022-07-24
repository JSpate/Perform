namespace Perform.Model.Console;

public interface IConsole : IDisposable
{
    LevelValue MasterVolume { get; }

    LevelValue MasterPan { get; }

    ITrack Track(int id);

    ITrack Track(string name);
}