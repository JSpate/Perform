namespace Perform.Interfaces;

public interface IDeviceItem: IEnumerable<object>
{
    public T Get<T>(string target);

    public bool Set<T>(string target, T value);
}