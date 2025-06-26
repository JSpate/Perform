using System.Text.Json.Serialization;

namespace Perform.Model;

[method: JsonConstructor]
public class DmxRangeDefinition(int address, string description, (int min, int max) range, int @default = 0)
{
    public int Address { get; } = address;

    public string Description { get; } = description;

    public (int min, int max) Range { get; } = range;

    public DmxRangeCategory Category { get; } = DmxRangeCategory.None;

    public int Default { get; } = @default;
}