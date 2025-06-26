using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Perform.Factories;
using Perform.Interfaces;
using Perform.Model;
using Perform.Model.Console;
using Perform.Web.Model;
using Timer = System.Timers.Timer;

namespace Perform.Web;

public class ShowService(
    DeviceFactory deviceFactory,
    IShowRepository showRepository,
    ISongRepository songRepository) : IDisposable
{
    private readonly ConcurrentDictionary<string, WebSocket> _webSockets = new();
    private readonly ShowState _state = new();
    private readonly ConcurrentDictionary<int, Song> _songs = new();
    private readonly ConcurrentDictionary<string, IDevice> _devices = new();
    private Show? _activeShow;
    private Timer? _beatTimer;
    private int _currentBeat;
    private int _currentBar;

    public async Task Handle(ButtonEvent notification, CancellationToken cancellationToken)
    {
        if (!_state.Active)
        {
            await HandleInactiveButtonEvent(notification);
        }
        else
        {
            await HandleActiveButtonEvent(notification);
        }
    }

    private async Task HandleInactiveButtonEvent(ButtonEvent buttonEvent)
    {
        switch (buttonEvent.ButtonId)
        {
            case 0 when buttonEvent.PressType == ButtonState.Press:
                await PreviousSong();
                break;
            case 1 when buttonEvent.PressType == ButtonState.Press:
                await NextSong();
                break;
            case 2 when buttonEvent.PressType == ButtonState.LongPress:
                // ToDo: implement a way to end the show
                break;
            case 3 when buttonEvent.PressType == ButtonState.LongPress:
                await StartSong();
                break;
        }
    }

    private async Task HandleActiveButtonEvent(ButtonEvent buttonEvent)
    {
        switch (buttonEvent.ButtonId)
        {
            case 0 when buttonEvent.PressType == ButtonState.Down:
            case 1 when buttonEvent.PressType == ButtonState.Down:
            case 2 when buttonEvent.PressType == ButtonState.Down:
            case 3 when buttonEvent.PressType == ButtonState.Down:
                HandleLiveButtonEvent(buttonEvent.ButtonId);
                break;
            case 3 when buttonEvent.PressType == ButtonState.LongPress:
                await EndSong();
                break;
        }
    }

    private void HandleLiveButtonEvent(int index)
    {
        foreach (var songConsole in _songs[_state.Song].Consoles)
        {
            if (songConsole.Stomp.TryGetValue(index, out var tracks) &&
                _devices.TryGetValue(songConsole.Name, out var device) && device is IConsole console)
            {
                console.SetState(tracks);
            }
        }
    }

    public async Task WebSocketHandler(HttpContext context, WebSocket webSocket)
    {
        if (_activeShow == null)
        {
            await GetAvailableShows();
        }
        else
        {
            _state.AvailableShows = null;
        }

        var socketId = Guid.NewGuid().ToString();
        _webSockets.TryAdd(socketId, webSocket);

        try
        {
            await SendStateAsync(webSocket);
            await ReceiveMessagesAsync(webSocket);
        }
        catch
        {
            // Ignore the error and cleanup
        }

        _webSockets.TryRemove(socketId, out _);
        webSocket.Dispose();
    }

    private async Task GetAvailableShows()
    {
        var shows = await showRepository.ListAsync();
        _state.AvailableShows = shows.ToArray();
    }

    private async Task SendStateAsync(WebSocket webSocket)
    {
        var stateJson = JsonSerializer.Serialize(_state, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var stateMessage = Encoding.UTF8.GetBytes(stateJson);
        await webSocket.SendAsync(new ArraySegment<byte>(stateMessage, 0, stateMessage.Length), WebSocketMessageType.Text, true, CancellationToken.None);
    }

    private async Task ReceiveMessagesAsync(WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];
        var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        while (!result.CloseStatus.HasValue)
        {
            var receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);

            var message = JsonSerializer.Deserialize<WebSocketMessage>(receivedMessage, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            if (message != null)
            {
                await HandleMessage(message);
            }

            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        }
        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
    }

    private async Task HandleMessage(WebSocketMessage message)
    {
        switch (message.Action)
        {
            case "selectShow":
                await HandleSelectShowMessage(message);
                break;
            case "nextSong":
                await NextSong();
                break;
            case "previousSong":
                await PreviousSong();
                break;
            case "startSong":
                await StartSong();
                break;
            case "endSong":
                await EndSong();
                break;
        }
    }

    private async Task HandleSelectShowMessage(WebSocketMessage message)
    {
        if (message.Value == null)
        {
            _activeShow = null;
        }
        else
        {
            _activeShow = await LoadShow(message.Value);
        }

        await SetShowState(message);
    }

    private async Task EndSong()
    {
        if (_activeShow != null)
        {
            _state.Active = false;
            _beatTimer?.Stop();
            if (_state.Song < _songs.Count - 1)
            {
                _state.Song++;
            }
            await SendStateAsync();
        }
    }

    private async Task PreviousSong()
    {
        if (_activeShow != null && _state.Song > 0)
        {
            _state.Song--;
            await SendStateAsync();
        }
    }

    private async Task NextSong()
    {
        if (_activeShow != null && _state.Song < _songs.Count - 1)
        {
            _state.Song++;
            await SendStateAsync();
        }
    }

    private async Task StartSong()
    {
        if (_activeShow == null)
        {
            return;
        }

        var song = _songs[_state.Song];

        foreach (var console in _devices.Values.OfType<IConsole>())
        {
            console.Tempo = song.BPM;
        }

        UpdateStompValues();

        _state.Active = true;
        StartBeatTimer(song.BPM, song.TimeSignature);
        await SendStateAsync();
    }

    private void StartBeatTimer(int? bpm, string? timeSignature)
    {
        if (bpm == null || string.IsNullOrEmpty(timeSignature))
        {
            return;
        }

        _currentBar = -1;
        _currentBeat = 0;
        var interval = 60000 / bpm.Value;
        var beatsPerMeasure = timeSignature switch
        {
            "4/4" => 4,
            "3/4" => 3,
            "5/8" => 5,
            "6/8" => 6,
            _ => 4
        };

        _beatTimer = new Timer(interval);
        _beatTimer.Elapsed += (_, _) =>
        {
            var isDownbeat = _currentBeat % beatsPerMeasure == 0;
            Task.Run(() => SendBeat(isDownbeat, interval, _currentBar));
            _currentBeat++;
            if (_currentBeat % beatsPerMeasure == 0)
            {
                _currentBar++;
            }
        };
        _beatTimer.Start();
    }

    private async Task SendBeat(bool isDownbeat, int interval, int bar)
    {
        var beatData = new
        {
            type = "beat",
            data = new
            {
                isDownbeat,
                interval,
                bar
            }
        };
        var beatJson = JsonSerializer.Serialize(beatData, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var beatMessage = Encoding.UTF8.GetBytes(beatJson);

        var tasks = _webSockets.Values.Select(socket =>
        {
            try
            {
                return socket.SendAsync(
                    new ArraySegment<byte>(beatMessage, 0, beatMessage.Length),
                    WebSocketMessageType.Text,
                    true, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception occurred sending a websocket message: {ex.Message}");
            }
            return Task.CompletedTask;
        });

        await Task.WhenAll(tasks);
    }

    private async Task SetShowState(WebSocketMessage message)
    {
        if (_activeShow == null)
        {
            _state.Show = null;
            _state.Description = null;
            _state.Song = -1;
        }
        else
        {
            _state.Show = message.Value;
            _state.Song = 0;
            _state.Songs = _songs.Select(s => new
            {
                name = s.Value.Name,
                description = s.Value.Description,
                bpm = s.Value.BPM,
                lyrics = s.Value.Lyrics,
                timeSignature = s.Value.TimeSignature
            });
        }

        _state.Active = false;
        await SendStateAsync();
    }

    private void UpdateStompValues()
    {
        //for (var index = 0; index < 4; index++)
        //{
        //    _state.Stomps[index] = null;
        //}

        //foreach (var songConsole in _songs[_state.Song].Consoles)
        //{
        //    for (var index = 0; index < 4; index++)
        //    {
        //        if (!songConsole.Stomp.TryGetValue(index, out var tracks) ||
        //            !_consoles.TryGetValue(songConsole.Name, out var console))
        //        {
        //            continue;
        //        }

        //        if (_state.Stomps[index] == null)
        //        {
        //            _state.Stomps[index] = console.GetState(tracks);
        //        }
        //        else
        //        {
        //            _state.Stomps[index] &= console.GetState(tracks);
        //        }
        //    }
        //}
    }

    private async Task<Show?> LoadShow(string? name)
    {
        _songs.Clear();
        foreach (var device in _devices.Values)
        {
            if (device is IConsole console)
            {
                console.ValueChanged -= ConsoleValueChanged;
            }

            device.Dispose();
        }
        
        _devices.Clear();

        if (string.IsNullOrWhiteSpace(name))
        {
            _activeShow = null;
            return null;
        }

        var show = await showRepository.GetByIdAsync(name);
        var consoleTasks = new List<Task>();

        if (show != null)
        {
            LoadShowDevices(show, consoleTasks);
            await LoadShowSongs(show);
        }

        await Task.WhenAll(consoleTasks);

        return show;
    }

    private async Task LoadShowSongs(Show show)
    {
        var index = 0;
        foreach (var songName in show.Songs)
        {
            try
            {
                var song = await songRepository.GetByIdAsync(songName);
                if (song != null && _songs.TryAdd(index, song))
                {
                    index++;
                }
            }
            catch
            {
                Console.WriteLine($"Error loading {songName}");
            }
        }
    }

    private void LoadShowDevices(Show show, List<Task> consoleTasks)
    {
        foreach (var deviceInfo in show.Devices)
        {
            var device = deviceFactory.CreateDevice(deviceInfo);
            if(device == null)
            {
                Console.WriteLine($"Could not create device of type {deviceInfo.Type}");
                continue;
            }

            _devices.TryAdd(deviceInfo.Id, device);

            if (device is IConsole console)
            {
                consoleTasks.Add(Task.Run(async () =>
                {
                    await Task.Delay(1000);
                    console.ValueChanged += ConsoleValueChanged;
                    //console.SetState(console.Tracks);
                }));
            }
        }
    }

    private void ConsoleValueChanged(int track, string propertyName)
    {
        if (!_state.Active)
        {
            return;
        }

        var stomps = _state.Stomps.Values.ToList();
        UpdateStompValues();
        if (!stomps.SequenceEqual(_state.Stomps.Values))
        {
            Task.Run(SendStateAsync);
        }
    }

    public async Task SendStateAsync()
    {
        var stateJson = JsonSerializer.Serialize(_state, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var stateMessage = Encoding.UTF8.GetBytes(stateJson);

        var tasks = _webSockets.Values.Select(socket =>
        {
            try
            {
                return socket.SendAsync(
                    new ArraySegment<byte>(stateMessage, 0, stateMessage.Length),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception occurred sending a websocket message: {ex.Message}");
            }
            return Task.CompletedTask;
        });

        await Task.WhenAll(tasks);
    }

    public void Dispose()
    {
        _beatTimer?.Dispose();
    }
}