namespace Perform.Model.Console;

public interface IFx
{
    public int Id { get; }

    public string? Preset { get; set; }

    IFxParameter Parameter(int fxId);
}