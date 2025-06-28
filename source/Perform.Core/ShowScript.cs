using Perform.Model;

namespace Perform
{
    public class ShowScript
    {
        public T Get<T>(string target)
        {
            throw new NotImplementedException();
        }

        public bool Set<T>(string target, T value)
        {
            throw new NotImplementedException();
        }

        public string TypeFor(string target)
        {
            var parts = target.Split('.', StringSplitOptions.RemoveEmptyEntries);

            switch (parts.Length)
            {
                case 1:
                    return "IDevice";
                case 2:
                    return "IDeviceItem";
                default:
                    return "double";
            }
        }

        public bool IsAvailable(string target)
        {
            throw new NotImplementedException();
        }

        public void AddEventHandler(Type eventHandler, EventType eventType, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public void RemoveEventHandler(Type eventHandler, EventType eventType, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public CurrentSong CurrentSong { get; set; } = new(0, 0, 0, 0, "", 0, "4/4");
    }
    
    public class CurrentSong(double time, int bar, int beat, int beatInBar, string title, int bpm, string timeSignature)
    {
        public double Time {get;} = time;

        public int Bar { get; } = bar;

        public int Beat { get; } = beat;

        public int BeatInBar { get; } = beatInBar;

        public string Title { get; } = title;

        public int BPM { get; } = bpm;

        public string TimeSignature { get; } = timeSignature;
    }
}
