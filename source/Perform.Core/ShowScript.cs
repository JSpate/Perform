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
            if (parts.Length == 2)
            {
                return "IDeviceItem";
            }

            if (parts.Length == 3)
            {
                return "int";
            }
            throw new ArgumentException("Target not found in the current configuration.");
        }

        public bool IsAvailable(string target)
        {
            throw new NotImplementedException();
        }

        public void AddEventHandler(Func<CancellationToken, Task> eventHandler, EventType eventType, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public void RemoveEventHandler(Func<CancellationToken, Task> eventHandler, EventType eventType, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public CurrentSong CurrentSong { get; set; } = new(0, 0, 0, 0, "", 0, "4/4");

        public LiteFunctions WithLites(CancellationToken cancellationToken, string[] lites)
        {
            throw new NotImplementedException();
        }

        public MotionFunctions WithMotion(CancellationToken cancellationToken, string[] lites)
        {
            throw new NotImplementedException();
        }
    }

    public class MotionFunctions
    {
        public void Circle(double changeAngle, int startX, int startY)
        {
            throw new NotImplementedException();
        }
    }

    public class LiteFunctions
    {
        public void Chase((double, double, double)[] colors)
        {
            throw new NotImplementedException();
        }
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
