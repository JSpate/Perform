namespace Perform.Model.Console;

public interface ITrack
{
    public int Id { get; }

    public string Name { get; }

    bool Mute { get; set; }

    LevelValue Pan { get; set; }

    LevelValue Volume { get; set; }

    IFx Fx(int fxId);
}