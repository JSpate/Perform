using System.Collections.Concurrent;

namespace Perform.Reaper;

public record Track(int Id)
{
    public int Id { get; set; } = Id;

    public string? Name { get; set; }

    public bool Mute { get; set; } = false;

    public float Volume { get; set; } = 0;

    public float Pan { get; set; } = 0;

    public bool Armed { get; set; } = false;

    public ConcurrentDictionary<int, Fx> Fx { get; } = new();
}