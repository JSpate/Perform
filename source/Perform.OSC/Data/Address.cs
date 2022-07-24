using System.Collections;

namespace Perform.OSC.Data;

public readonly struct Address : IEnumerable<string>
{
    private readonly string _address;
    private readonly string[] _parts;

    private Address(string address)
    {
        _address = address;
        _parts = address.ToLowerInvariant().Trim('/').Split('/');
    }

    public string this[int index] => _parts[index];
    
    public static implicit operator string(Address address)
    {
        return address._address;
    }

    public static implicit operator Address(string address)
    {
        return new Address(address);
    }

    public IEnumerator<string> GetEnumerator()
    {
        foreach (var part in _parts)
        {
            yield return part;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _parts.GetEnumerator();
    }
}