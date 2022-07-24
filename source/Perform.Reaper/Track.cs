using Perform.Model.Console;

namespace Perform.Reaper;

public record Track
{
    private readonly Dictionary<int, string> _fx;

    private Track(int id, string name, bool mute, float volume, float level, float pan, Dictionary<int, string> fx,  int? fxId = null, string fxName = "")
    {
        Id = id;
        Name = name;
        Mute = mute;
        Volume = volume;
        Level = level;
        Pan = pan;
        
        if (fxId != null)
        {
            fx[fxId.Value] = fxName;
        }

        _fx = fx;
    }

    public int Id { get; }

    public string Name { get; }

    public bool Mute { get; }

    public LevelValue Volume { get; }

    public LevelValue Level { get; }

    public LevelValue Pan { get; }

    public IReadOnlyDictionary<int, string> FxPresets => _fx;

    public static Track Create(int id, string name = "", bool mute = false, float volume = 0, float level = 0, float pan = .5f, int? fxId = null, string fxName = "")
    {
        var fx = new Dictionary<int, string>
        {
            { 1, "" },
            { 2, "" },
            { 3, "" },
            { 4, "" },
            { 5, "" },
            { 6, "" },
            { 7, "" },
            { 8, "" }
        };

        return new Track(id, name, mute, volume, level, pan, fx, fxId, fxName);
    }

    public static Track UpdateName(Track track, string name)
    {
        return new Track(track.Id, name, track.Mute, track.Volume, track.Level, track.Pan, track._fx);
    }

    public static Track UpdateMute(Track track, bool mute)
    {
        return new Track(track.Id, track.Name, mute, track.Volume, track.Level, track.Pan, track._fx);
    }

    public static Track UpdateVolume(Track track, float volume)
    {
        Console.WriteLine($"Track {track.Id}({track.Name} Volume: {volume}");
        return new Track(track.Id, track.Name, track.Mute, volume, track.Level, track.Pan, track._fx);
    }

    public static Track UpdateLevel(Track track, float level)
    {
        return new Track(track.Id, track.Name, track.Mute, track.Volume, level, track.Pan, track._fx);
    }

    public static Track UpdatePan(Track track, float pan)
    {
        return new Track(track.Id, track.Name, track.Mute, track.Volume, track.Level, pan, track._fx);
    }

    public static Track UpdateFx(Track track, int fxId, string fxName)
    {
        return new Track(track.Id, track.Name, track.Mute, track.Volume, track.Level, track.Pan, track._fx, fxId, fxName);
    }
}