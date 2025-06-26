namespace Perform.Model.Console;

public interface IFxParameter
{
    public int Id { get; }

    public float Value { get; set; }

    public void Toggle();
}