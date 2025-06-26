namespace Perform.Model.Console;

public interface ITrack
{
    public int Id { get; }

    public string? Name { get; }

    bool Mute { get; set; }

    bool Armed { get; set; }

    float Pan { get; set; }

    float Volume { get; set; }

    IFx Fx(int fxId);
}