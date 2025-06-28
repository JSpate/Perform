namespace Perform.Interfaces;

public interface IDevice : IEnumerable<IDeviceItem>, IDisposable
{
    public T Get<T>(string target);

    public bool Set<T>(string target, T value);
}