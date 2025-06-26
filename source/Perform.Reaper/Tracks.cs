using System.Collections.Concurrent;

namespace Perform.Reaper;

public sealed class Tracks
{
    private readonly ConcurrentDictionary<int, Track> _byId = new();

    public Track this[int index] => _byId[index];

    public Track Update(int id, Action<Track> update)
    {
        var track = _byId.GetOrAdd(id, new Track(id));
        update(track);
        return track;
    }

    public void UpdateFx(int id, int fxId, Action<Fx> updateFx)
    {
        Update(id, t =>
        {
            var fx = t.Fx.GetOrAdd(fxId, new Fx(fxId));
            updateFx(fx);
        });
    }

    public bool HasId(int id) => _byId.ContainsKey(id);

    public bool TryGetId(string name, out int id)
    {
        foreach (var track in _byId.Values)
        {
            if (track.Name == name)
            {
                id = track.Id;
                return true;
            }
        }
        id = -1;
        return false;
    }
}