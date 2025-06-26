namespace Perform.Reaper;

public class FxParameter(int id)
{
    public int Id { get; set; } = id;

    public float Value { get; set; } = 0f;
}