using System.Collections;
using System.Text.Json;
using Perform.Interfaces;
using Perform.Model;
using Perform.Model.Console;
using Perform.OSC;
using Perform.OSC.Data;

namespace Perform.Reaper;

public sealed class Reaper : IMidiConsole
{
    public event ValueChangedEventHandler? ValueChanged;

    private delegate void SendDelegate(string address, params object?[] args);

    private readonly IClient _oscClient;
    private readonly Tracks _tracks = new();
    private Task? _processTask;
    private int? _tempo;

    public Reaper(string name, OscConfig config)
    {
        Name = name;
        _oscClient = new ClientFactory()
            .Create(config, MessageReceived);

        // Reset OSC
        Send("i/action", 41743);
    }

    public string Name { get; }

    public float MasterVolume { get; private set; }

    public float MasterPan { get; private set; }

    public int? Tempo
    {
        get => _tempo;
        set
        {
            if (_tempo != value)
            {
                _tempo = value;
                if (_tempo != null)
                {
                   Send("/tempo/raw", (float)_tempo);
                }
            }
        }
    }

    public bool TryGetTrack(int id, out ITrack track)
    {
        if (_tracks.HasId(id))
        {
            track = new TrackWrapper(Send, ValueChanged, _tracks, id);
            return true;
        }
        track = null!;
        return false;
    }

    public bool TryGetTrack(string name, out ITrack track)
    {
        if (_tracks.TryGetId(name, out var id))
        {
            track = new TrackWrapper(Send, ValueChanged, _tracks, id);
            return true;
        }
        track = null!;
        return false;
    }

    public bool? GetState(Dictionary<string, Dictionary<string, JsonElement>> state)
    {
        bool? result = null;
        void UpdateResult(bool value)
        {
            if (result == null)
            {
                result = value;
            }
            else
            {
                result &= value;
            }
        }

        foreach (var trackEntry in state)
        {
            var trackName = trackEntry.Key;
            if (!TryGetTrack(trackName, out var track))
            {
                continue;
            }

            foreach (var propertyEntry in trackEntry.Value)
            {
                var propertyPath = propertyEntry.Key.Split('.');
                var value = propertyEntry.Value;

                switch (propertyPath[0])
                {
                    case "armed":
                        UpdateResult(track.Armed == value.GetBoolean());
                        break;
                    case "mute":
                        UpdateResult(track.Mute = value.GetBoolean());
                        break;
                    case "volume":
                        if (value.TryGetInt32(out var volume))
                        {
                            UpdateResult(Math.Abs(track.Volume - volume) < 0.0001);
                        }
                        break;
                    case "pan":
                        if (value.TryGetInt32(out var pan))
                        {
                            UpdateResult(Math.Abs(track.Pan - pan) < 0.0001);
                        }
                        break;
                    case "fx":
                        if (propertyPath.Length >= 3 && int.TryParse(propertyPath[1], out var fxId))
                        {
                            var fx = track.Fx(fxId);
                            switch (propertyPath[2])
                            {
                                case "preset":
                                    UpdateResult(fx.Preset == value.GetString());
                                    break;
                                case "fxparam":
                                    switch (propertyPath[4])
                                    {
                                        case "value":
                                            if (propertyPath.Length == 5 &&
                                                int.TryParse(propertyPath[3], out var paramId))
                                            {
                                                float paramValue;
                                                if (value.ValueKind == JsonValueKind.True)
                                                {
                                                    paramValue = 0;
                                                }
                                                else if (value.ValueKind == JsonValueKind.False)
                                                {
                                                    paramValue = 1;
                                                }
                                                else if (value.ValueKind == JsonValueKind.Number)
                                                {
                                                    paramValue = value.GetSingle();
                                                }
                                                else
                                                {
                                                    break;
                                                }

                                                var parameter = fx.Parameter(paramId);
                                                UpdateResult(Math.Abs(parameter.Value - paramValue) < 0.0001);
                                            }
                                            break;

                                        case "toggle":
                                            if (propertyPath.Length == 5 && int.TryParse(propertyPath[3], out paramId))
                                            {
                                                var parameter = fx.Parameter(paramId);
                                                UpdateResult(parameter.Value == 0);
                                            }
                                            break;
                                    }

                                    break;
                            }
                        }
                        break;
                }
            }
        }

        return result;
    }

    public void SetState(Dictionary<string, Dictionary<string, JsonElement>> state)
    {
        foreach (var trackEntry in state)
        {
            var trackName = trackEntry.Key;
            if (!TryGetTrack(trackName, out var track))
            {
                continue;
            }

            foreach (var propertyEntry in trackEntry.Value)
            {
                var propertyPath = propertyEntry.Key.Split('.');
                var value = propertyEntry.Value;

                switch (propertyPath[0])
                {
                    case "armed":
                        track.Armed = value.GetBoolean();
                        break;
                    case "mute":
                        track.Mute = value.GetBoolean();
                        break;
                    case "volume":
                        if (value.TryGetInt32(out var volume))
                        {
                            track.Volume = volume;
                        }
                        if (value.TryGetSingle(out var volumeS))
                        {
                            track.Volume = volumeS;
                        }
                        break;
                    case "pan":
                        if (value.TryGetInt32(out var pan))
                        {
                            track.Pan = pan;
                        }
                        if (value.TryGetSingle(out var panS))
                        {
                            track.Pan = panS;
                        }
                        break;
                    case "fx":
                        if (propertyPath.Length >= 3 && int.TryParse(propertyPath[1], out var fxId))
                        {
                            var fx = track.Fx(fxId);
                            switch (propertyPath[2])
                            {
                                case "preset":
                                    fx.Preset = value.GetString();
                                    break;
                                case "fxparam":
                                    switch (propertyPath[4])
                                    {
                                        case "value":
                                            if (propertyPath.Length == 5 &&
                                                int.TryParse(propertyPath[3], out var paramId))
                                            {
                                                float paramValue;
                                                if (value.ValueKind == JsonValueKind.True)
                                                {
                                                    paramValue = 0;
                                                }
                                                else if (value.ValueKind == JsonValueKind.False)
                                                {
                                                    paramValue = 1;
                                                }
                                                else if (value.ValueKind == JsonValueKind.Number)
                                                {
                                                    paramValue = value.GetSingle();
                                                }
                                                else
                                                {
                                                    break;
                                                }

                                                var parameter = fx.Parameter(paramId);
                                                parameter.Value = paramValue;
                                            }

                                            break;

                                        case "toggle":
                                            if (propertyPath.Length == 5 && int.TryParse(propertyPath[3], out paramId))
                                            {
                                                fx.Parameter(paramId).Toggle();
                                            }

                                            break;
                                    }

                                    break;
                            }
                        }
                        break;
                }
            }
        }
    }

    public void SendMidiNote(int channel, int note, int value)
    {
        Send($"i/vkb_midi/{channel}/note/{note}", value);
    }

    public void SendMidiControlChange(int channel, int change, int value)
    {
        Send($"i/vkb_midi/{channel}/cc/{change}", value);
    }

    private Task MessageReceived(Packet? packet)
    {
        if (packet != null)
        {
            foreach (var msg in packet.Messages)
            {
                try
                {
                    switch (msg.Address[0])
                    {
                        case "tempo" when msg.Address is [_, "raw"] && msg.Arguments[0] is float v:
                            Tempo = Convert.ToInt32(v);
                            break;
                        case "master":
                            switch (msg.Address[1])
                            {
                                case "volume" when msg.Arguments[0] is float v:
                                    MasterVolume = v;
                                    break;
                                case "pan" when msg.Arguments[0] is float p:
                                    MasterPan = p;
                                    break;
                            }
                            break;

                        case "track":
                            if (int.TryParse(msg.Address[1], out var track))
                            {
                                UpdateTrack(track, msg);
                            }
                            break;
                    }
                }
                catch (Exception)
                {
                    // Ignore
                }
            }
        }
        return Task.CompletedTask;
    }

    private void Send(string address, params object?[] args)
    {
        _oscClient.Send(new Message(address, "", args));
    }

    public void Dispose()
    {
        _processTask?.Dispose();
        _processTask = null;

        _oscClient.Dispose();
    }

    private void UpdateTrack(int id, Message msg)
    {
        switch (msg.Address[2])
        {
            case "name":
                _tracks.Update(
                    id,
                    t => t.Name = msg.Arguments[0]?.ToString() ?? string.Empty);
                break;
            case "volume" when msg.Arguments[0] is float v:
                _tracks.Update(id, t => t.Volume = v);
                ValueChanged?.Invoke(id, "volume");
                break;
            case "pan" when msg.Arguments[0] is float v:
                _tracks.Update(id, t => t.Pan = v);
                ValueChanged?.Invoke(id, "pan");
                break;
            case "vu":

                break;
            case "mute" when msg.Arguments[0] is float v:
                _tracks.Update(id, t => t.Mute = v > 0);
                ValueChanged?.Invoke(id, "mute");
                break;
            case "recarm" when msg.Arguments[0] is float v:
                _tracks.Update(id, t => t.Armed = v > 0);
                ValueChanged?.Invoke(id, "armed");
                break;
            case "fx":
                var fxId = int.Parse(msg.Address[3]);
                _tracks.UpdateFx(id, fxId, fx =>
                {
                    switch (msg.Address[4])
                    {
                        case "name":
                            fx.Name = (string)(msg.Arguments[0] ?? "");
                            break;
                        case "preset":
                            fx.Preset = (string)(msg.Arguments[0] ?? "");
                            ValueChanged?.Invoke(id, $"fx/{fxId}/preset");
                            break;
                        case "fxparam":
                            var param = fx.Parameters.GetOrAdd(int.Parse(msg.Address[5]), i => new FxParameter(i));
                            if (msg.Address[6] == "value")
                            {
                                param.Value = (float)(msg.Arguments[0] ?? 0f);
                                ValueChanged?.Invoke(id, $"fx/{fxId}/fxparam/{param.Id}");
                            }
                            break;
                    }
                });
                break;
        }
    }

    private class TrackWrapper(SendDelegate send, ValueChangedEventHandler? changedEvent, Tracks tracks, int id) : ITrack
    {
        public int Id => id;

        public string? Name => tracks[id].Name;

        public bool Mute
        {
            get => tracks[id].Mute;
            set
            {
                if (tracks[id].Mute == value)
                {
                    return;
                }
                send($"/track/{id}/mute", value ? 1f : 0f);
                tracks[id].Mute = value;
                changedEvent?.Invoke(id, "mute");
            }
        }

        public bool Armed
        {
            get => tracks[id].Armed;
            set
            {
                if (tracks[id].Armed == value)
                {
                    return;
                }

                send($"/track/{id}/recarm", value ? 1f : 0f);
                tracks[id].Armed = value;
                changedEvent?.Invoke(id, "armed");
            }
        }

        public float Pan
        {
            get => tracks[id].Pan;
            set
            {
                if (Math.Abs(tracks[id].Pan - value) < 0.0001)
                {
                    return;
                }
                send($"/track/{id}/pan", value);
                tracks[id].Pan = value;
                changedEvent?.Invoke(id, "pan");
            }
        }

        public float Volume
        {
            get => tracks[id].Volume;
            set
            {
                if (Math.Abs(tracks[id].Volume - value) < 0.0001)
                {
                    return;
                }
                send($"/track/{id}/volume", value);
                tracks[id].Volume = value;
                changedEvent?.Invoke(id, "volume");
            }
        }

        public IFx Fx(int fxId)
        {
            return new FxWrapper(send, changedEvent, tracks, id, fxId);
        }

        private class FxWrapper(SendDelegate send, ValueChangedEventHandler? changedEvent, Tracks tracks, int trackId, int fxId) : IFx
        {
            public int Id => fxId;

            public string? Preset
            {
                get => tracks[trackId].Fx[fxId].Preset;
                set
                {
                    if (tracks[trackId].Fx[fxId].Preset == value)
                    {
                        return;
                    }

                    send($"/track/{trackId}/fx/{fxId}/preset", value);
                    tracks[trackId].Fx[fxId].Preset = value;
                    changedEvent?.Invoke(trackId, $"fx/{fxId}/preset");
                }
            }

            public IFxParameter Parameter(int parameterId)
            {
                return new FxParameterWrapper(send, changedEvent, tracks, trackId, fxId, parameterId);
            }
        }

        private class FxParameterWrapper(
            SendDelegate send,
            ValueChangedEventHandler? changedEvent,
            Tracks tracks,
            int trackId,
            int fxId,
            int parameterId) : IFxParameter
        {
            public int Id => parameterId;

            public float Value
            {
                get => tracks[trackId].Fx[fxId].Parameters[parameterId].Value;
                set
                {
                    var param = tracks[trackId].Fx[fxId].Parameters[parameterId];
                    if (Math.Abs(param.Value - value) < 0.0001)
                    {
                        return;
                    }

                    param.Value = value;
                    send($"/track/{trackId}/fx/{fxId}/fxparam/{parameterId}/value", value);
                    changedEvent?.Invoke(trackId, $"fx/{fxId}/fxparam/{parameterId}");
                }
            }

            public void Toggle()
            {
                var param = tracks[trackId].Fx[fxId].Parameters[parameterId];
                var newValue = param.Value < 0.5f ? .9f : 0;
                Value = newValue;
            }
        }
    }

    public IEnumerator<IDeviceItem> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public T Get<T>(string target)
    {
        throw new NotImplementedException();
    }

    public bool Set<T>(string target, T value)
    {
        throw new NotImplementedException();
    }
}
