using System.Collections.Concurrent;

namespace Perform.Reaper;

public record Fx(int Id)
{
    public string? Name { get; set; }

    public string? Preset { get; set; }

    public ConcurrentDictionary<int, FxParameter> Parameters { get; } = new();
}