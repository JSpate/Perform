namespace Perform.Model;

public class ChaseProperties
{
    public ChaseProperties(string trigger, IDictionary<string, int[]> properties)
    {
        Trigger = trigger;
        Properties = properties;
    }

    public string Trigger { get; }

    public IDictionary<string, int[]> Properties { get; }
}