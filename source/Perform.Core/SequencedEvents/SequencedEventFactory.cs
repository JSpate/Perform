namespace Perform.SequencedEvents;

internal class SequencedEventFactory
{
    private delegate ISequencedEvent Creator(IList<object> args);

    private static readonly Dictionary<string, Creator> Creators = new()
    {
        { "delay", CreateDelay },
        { "midiCC", CreateMidiControlChange },
        { "midiNote", CreateMidiNote },
        { "mute", CreateMute },
        { "setFxPreset", CreateSetFxPreset },
        { "setLight", CreateSetLight },
        { "unmute", CreateUnmute },
        { "volumeChange", CreateVolume}
    };

    public static ISequencedEvent Create(string name, IList<object> args)
    {
        if(Creators.TryGetValue(name, out var creator))
        {
            return creator(args);
        }

        throw new ArgumentException($"A creator with the name {name} was not found", nameof(name));
    }

    private static ISequencedEvent CreateDelay(IList<object> args)
    {
        if (args.Count == 1 && args[0] is int delay)
        {
            return new Delay(delay);
        }

        throw new ArgumentException("Delay args must be in the form [1000] (delay in milliseconds");
    }

    private static ISequencedEvent CreateMidiControlChange(IList<object> args)
    {
        if (args.Count == 3 && args[0] is int channel && args[1] is int change && args[2] is int value)
        {
            return new MidiControlChange(channel, change, value);
        }

        throw new ArgumentException("MidiCC args must be in the form [0,10,255] (channel, change and value");
    }

    private static ISequencedEvent CreateMidiNote(IList<object> args)
    {
        if (args.Count == 3 && args[0] is int channel && args[1] is int note && args[2] is int value)
        {
            return new MidiNote(channel, note, value);
        }

        throw new ArgumentException("MidiNote args must be in the form [0,10,255] (channel, note and value");
    }

    private static ISequencedEvent CreateMute(IList<object> args)
    {
        if (args.Count == 1 && args[0] is int trackId)
        {
            return new Mute(trackId);
        }

        throw new ArgumentException("Mute args must be in the form [1] (track id");
    }

    private static ISequencedEvent CreateSetFxPreset(IList<object> args)
    {
        if (args.Count == 3 && args[0] is int trackId && args[1] is int fxId && args[2] is string preset)
        {
            return new SetFxPreset(trackId, fxId, preset);
        }

        throw new ArgumentException("Set FX preset args must be in the form [1, 1, \"preset\"] (trackId, fxId, preset name");
    }

    private static ISequencedEvent CreateSetLight(IList<object> args)
    {
        if (args.Count == 5 && args[0] is string name && args[1] is string from && args[2] is string to && args[3] is int fadeIn &&
            args[4] is int fadeOut)
        {
            return new SetLight(name, from, to, fadeIn, fadeOut);
        }

        throw new ArgumentException(
            "Set light args must be in the form [\"name\", \"R=0,B=0\", \"R=255,B=127\", 100, 200] (light name, from, to, fadeIn time in milliseconds and fade out time in milliseconds");
    }

    private static ISequencedEvent CreateVolume(IList<object> args)
    {
        if (args.Count == 2 && args[0] is int track && args[1] is int change)
        {
            return new VolumeChange(track, change);
        }

        throw new ArgumentException("Volume change args must be in the form [1, -2] (track, change");
    }

    private static ISequencedEvent CreateUnmute(IList<object> args)
    {
        if (args.Count == 1 && args[0] is int trackId)
        {
            return new Unmute(trackId);
        }

        throw new ArgumentException("Unmute args must be in the form [1] (track id");
    }
}