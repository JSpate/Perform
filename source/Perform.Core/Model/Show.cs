using Perform.Config;
using Perform.DMX;
using Perform.Model.Console;
using Perform.SequencedEvents;
using Perform.Services;

namespace Perform.Model;

public class Show : IDisposable
{
    private readonly ButtonEventService _buttons;
    private int _songIndex;

    public event SongChangeEvent? SongChange;

    public Show(
        IList<string> lightPositions,
        IList<Song> songs, 
        IDictionary<string, Trigger> triggers, 
        IDictionary<string, Sequence> sequences, 
        IDictionary<string, IList<ChaseLight>> chases,
        IList<IConsole> consoles,
        string dmxPort)
    {
        LightPositions = lightPositions;
        Triggers = triggers;
        Sequences = sequences;
        Chases = chases;
        Consoles = consoles;

        _buttons = new ButtonEventService(AsyncHelper.RunSync(ConfigProvider.LoadButtons));

        _buttons.ButtonEvent += ButtonsOnButtonEvent; 

        Dmx = AsyncHelper.RunSync(ConfigProvider.LoadDmxUniverse);

        if (Dmx == null)
        {
            throw new SerializationException("Dmx deserialization failed");
        }

        Dmx.Initialize(dmxPort);

        Songs = songs;

        State = ShowState.Waiting;
    }

    public IList<string> LightPositions { get; }
    
    public IList<Song> Songs { get; }
    
    public IDictionary<string, Trigger> Triggers { get; }
    
    public IDictionary<string, Sequence> Sequences { get; }
    
    public IDictionary<string, IList<ChaseLight>> Chases { get; }

    public IList<IConsole> Consoles { get; }

    public DmxUniverse Dmx { get; }

    public void Start()
    {
        _buttons.Start();
        State = ShowState.SongSelect;
    }

    public void Stop()
    {
        _buttons.Stop();
    }

    public ShowState State { get; private set; }

    public Song? CurrentSong => _songIndex > 0 && _songIndex < Songs.Count ? Songs[_songIndex] : null;

    public void Dispose()
    {
        _buttons.Dispose();
    }

    private void ButtonsOnButtonEvent(ButtonPressType type, int button)
    {
        switch (State)
        {
            case ShowState.SongSelect:
                if (type == ButtonPressType.Up)
                {
                    switch (button)
                    {
                        case 0:
                            SongIndex--;
                            break;
                        case 1:
                            SongIndex++;
                            break;
                        case 3:
                            StartSong();
                            break;
                    }
                }
                break;

            case ShowState.InSong:
                break;

            default:
                throw new Exception("A button event occurred whilst the show was not in an active state");
        }
    }

    private int SongIndex
    {
        get => _songIndex;
        set
        {
            if (value < 0)
            {
                value = Songs.Count - 1;
            }

            if (value >= Songs.Count)
            {
                value = 0;
            }

            if (_songIndex != value)
            {
                _songIndex = value;
                SongChange?.Invoke(Songs[_songIndex]);
            }
        }
    }

    private void StartSong()
    {
        State = ShowState.InSong;
    }
}

public delegate void SongChangeEvent(Song song);

public enum ShowState
{
    Waiting,
    SongSelect,
    InSong
}