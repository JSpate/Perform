using System.Collections.Concurrent;
using Perform.Model;
using Perform.Model.Console;
using Perform.OSC;
using Perform.OSC.Data;
using ReaperTrack = Perform.Reaper.Track;

namespace Perform.Reaper;

public sealed class Reaper : IMidiConsole
{
    private delegate void SendDelegate(string address, params object?[] args);

    private readonly ManualResetEventSlim _reset = new(true);
    private readonly IClient _oscClient;
    private readonly Tracks _tracks = new();
    private readonly ConcurrentQueue<Message> _toProcess = new();
    private Task? _processTask;
    private LevelValue _masterVolume;
    private LevelValue _masterPan;
    
    public Reaper(string name, OscConfig config)
    {
        Name = name;
        _oscClient = new ClientFactory()
            .Create(
                config,
                MessageReceived);
        
        // Reset OSC
        Send("i/action", 41743);
    }

    public string Name { get; }

    public LevelValue MasterVolume => _masterVolume;

    public LevelValue MasterPan => _masterPan;

    public ITrack Track(int id)
    {
        return new TrackWrapper(Send, _tracks, id);
    }

    public ITrack Track(string name)
    {
        return Track(_tracks[name].Id);
    }

    public void SendMidiNote(int channel, int note, int value)
    {
        Send($"i/midi/{channel}/{note}", value);
    }

    public void SendMidiControlChange(int channel, int change, int value)
    {
        Send($"i/midi/cc/{channel}/{change}", value);
    }
    
    private Task MessageReceived(Packet? packet)
    {
        if (packet != null)
        {
            foreach (var msg in packet.Messages)
            {
                Enqueue(msg);
            }
        }
        return Task.CompletedTask;
    }

    private void Enqueue(Message msg)
    {
        _toProcess.Enqueue(msg);

        if (_reset.IsSet)
        {
            _processTask = ProcessQueue().ContinueWith(_ => _reset.Set());
        }
    }

    private Task ProcessQueue()
    {
        _reset.Reset();

        while (_toProcess.TryDequeue(out var msg))
        {
            switch (msg.Address[0])
            {
                case "master":
                    switch (msg.Address[1])
                    {
                        case "volume":
                            _masterVolume = (float)(msg.Arguments[0] ?? 0);
                            break;
                        case "pan":
                            _masterPan = (float)(msg.Arguments[0] ?? 127);
                            break;
                    }
                    break;
                case "device":
                    break;
                default:
                    var track = int.Parse(msg.Address[0]);
                    UpdateTrack(track, msg);
                    break;
            }
        }

        return Task.CompletedTask;
    }

    private void Send(string address, params object?[] args)
    {
        _oscClient.Send(new Message(address, args));
    }

    public void Dispose()
    {
        _processTask?.Dispose();
        _processTask = null;

        _oscClient.Dispose();
    }

    private void UpdateTrack(int id, Message msg)
    {
        switch (msg.Address[1])
        {
            case "name":
                _tracks.AddUpdate(
                    id,
                    i => ReaperTrack.Create(i, name: (string)(msg.Arguments[0] ?? "")),
                    (_, t) => ReaperTrack.UpdateName(t, (string)(msg.Arguments[0] ?? "")));
                break;
            case "volume":
                _tracks.AddUpdate(
                    id,
                    i => ReaperTrack.Create(i, volume: (float)(msg.Arguments[0] ?? 0f)),
                    (_, t) => ReaperTrack.UpdateVolume(t, (float)(msg.Arguments[0] ?? 0f)));
                break;
            case "pan":
                _tracks.AddUpdate(
                    id,
                    i => ReaperTrack.Create(i, pan: (float)(msg.Arguments[0] ?? 0.5f)),
                    (_, t) => ReaperTrack.UpdatePan(t, (float)(msg.Arguments[0] ?? 0.5f)));
                break;
            case "vu":
                _tracks.AddUpdate(
                    id,
                    i => ReaperTrack.Create(i, level: (float)(msg.Arguments[0] ?? 0f)),
                    (_, t) => ReaperTrack.UpdateLevel(t, (float)(msg.Arguments[0] ?? 0f)));
                break;
            case "mute":
                _tracks.AddUpdate(
                    id,
                    i => ReaperTrack.Create(i, mute: msg.Arguments[0]?.Equals(1f) ?? false),
                    (_, t) => ReaperTrack.UpdateMute(t, msg.Arguments[0]?.Equals(1f) ?? false));
                break;
            case "fx":
                var fxId = int.Parse(msg.Address[2]);
                _tracks.AddUpdate(
                    id,
                    i => ReaperTrack.Create(i, fxId: fxId, fxName: (string)(msg.Arguments[0] ?? "")),
                    (_, t) => (ReaperTrack.UpdateFx(t, fxId, (string)(msg.Arguments[0] ?? ""))));
                break;
        }
    }

    private class TrackWrapper : ITrack
    {
        private readonly int _id;
        private readonly Tracks _tracks;
        private readonly SendDelegate _send;

        public TrackWrapper(SendDelegate send, Tracks tracks, int id)
        {
            _send = send;
            _tracks = tracks;
            _id = id;
        }

        public int Id => _id;

        public string Name => _tracks[_id].Name;

        public bool Mute
        {
            get => _tracks[_id].Mute; 
            set=> _send($"b/{(_id)}/mute", value ? 1 : 0);
        }

        public LevelValue Pan {
            get => _tracks[_id].Pan;
            set => _send($"b/{(_id)}/pan", (float)value);
        }

        public LevelValue Volume
        {
            get => _tracks[_id].Pan;
            set => _send($"b/{(_id)}/volume", (float)value);
        }

        public IFx Fx(int fxId)
        {
            return new FxWrapper(_send, _tracks, _id, fxId);
        }

        private class FxWrapper : IFx
        {
            private readonly SendDelegate _send;
            private readonly Tracks _tracks;
            private readonly int _trackId;

            public FxWrapper(SendDelegate send, Tracks tracks, int trackId, int fxId)
            {
                _send = send;
                _tracks = tracks;
                _trackId = trackId;
                Id = fxId;
            }

            public int Id { get; }

            public string Preset
            {
                get => _tracks[_trackId].FxPresets[Id];
                set => _send($"s/{_trackId}/fx/{Id}/preset", value);
            }
        }
    }
}

