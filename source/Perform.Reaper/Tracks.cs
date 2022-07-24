using System.Collections.Concurrent;

namespace Perform.Reaper;

public sealed class Tracks
{
    private readonly ConcurrentDictionary<int, Track> _byId;
    private readonly ConcurrentDictionary<string, Track> _byName;

    public Tracks()
    {
        _byId = new ConcurrentDictionary<int, Track>();
        _byName = new ConcurrentDictionary<string, Track>();
    }

    public Track this[int index] => _byId[index];

    public Track this[string name] => _byName[name.ToLowerInvariant()];

    public Track AddUpdate(int id, Func<int, Track> add, Func<int, Track, Track> update)
    {
        var track = _byId.AddOrUpdate(id, add, update);

        if (!string.IsNullOrWhiteSpace(track.Name))
        {
            _byName[track.Name.ToLowerInvariant()] = track;
        }

        return track;
    }

}