using System.Collections.Concurrent;
using Perform.Model.Console;
using Websocket.Client;

namespace Perform.UI24R;

public class Client(string uri)
{
    private readonly Uri _uri = new(uri);
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<int, ITrack>>? _tracks = new();
    private WebsocketClient? _client;

    /// <summary>
    ///  Represent the bridge between the UI24R and a DAW controller
    /// </summary>
    public void Connect()
    {
        _client = new WebsocketClient(_uri);
        _client.MessageReceived.Subscribe(m =>
        {
            if (m.Text == null)
            {
                return;
            }
            if (m.Text.Length > 3)
            {
                foreach (var part in m.Text.Substring(4).Split("\n"))
                {
                    MessageReceived(part);
                }
            }
            else
            {
                _client.Send(m.Text);
                _client.Send("3:::ALIVE");
            }
        });
        _client.DisconnectionHappened.Subscribe(WebsocketDisconnectionHappened);
        _client.ReconnectionHappened.Subscribe(WebsocketReconnectionHappened);
        _client.ErrorReconnectTimeout = new TimeSpan(0, 0, 10);

        _client.Start();

    }
    private void WebsocketReconnectionHappened(ReconnectionInfo info)
    {
    }
    private void WebsocketDisconnectionHappened(DisconnectionInfo info)
    {
    }

    public void Dispose()
    {
        if (_client != null)
        {
            _client.Dispose();
            _client = null;
        }
    }

    private void MessageReceived(string message)
    {

        var itemParts = message.Split('^', 2);

        switch (itemParts[0])
        {
            case "SETD":
            case "SETS":
                Console.WriteLine($"Received message: {message}");
                //UpdateData(itemParts[1]);
                break;

            case "VU2":
                var data = new RealTimeData(itemParts[1]);
                break;
        }
    }

    private void UpdateData(string data)
    {
        var parts = data.Split('.');
        ConcurrentDictionary<int, ITrack> group;

        switch (parts[0])
        {
            case "i":
                group = _tracks!.GetOrAdd("Input", []);
                break;
            case "m":
                group = _tracks!.GetOrAdd("Master", []);
                break;

            default:
                return;
        }

        var track = group.GetOrAdd(int.Parse(parts[1]), id => new Track(id));
        
        if (parts[2].StartsWith("v^"))
        {
            track.Volume = float.Parse(parts[2].Substring(4));
        }

    }

    public void SendMessage(string message)
    {
        _client?.Send(message);
    }

    public List<double> ParseRealTimeAudioData(string a)
    {
        var bytes = Convert.FromBase64String(a);
        return bytes.Select(b => 0.004167508166392142 * b).ToList();
    }

    public ConcurrentDictionary<string, ConcurrentDictionary<int, ITrack>>? Tracks => _tracks;
}

public class Track(int id) : ITrack
{
    public int Id { get; set; } = id;

    public string? Name { get; }

    public bool Mute { get; set; }

    public bool Armed { get; set; }

    public float Pan { get; set; }

    public float Volume { get; set; }

    public IFx Fx(int fxId)
    {
        throw new NotImplementedException();
    }
    
}
