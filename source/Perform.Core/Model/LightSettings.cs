namespace Perform.Model;

public readonly struct LightSettings
{
    private readonly Dictionary<string, byte> _data = new();

    private LightSettings(string settings)
    {
        var items = settings.Trim().Split(',');
        foreach (var item in items.Select(i=>i.Trim().Split('=', 2)))
        {
            if (item.Length == 2 && byte.TryParse(item[1], out var value))
            {
                _data.Add(item[0], value);
            }
        }
    }

    public IReadOnlyDictionary<string, byte> Settings => _data;

    public override string ToString()
    {
        return _data.Select(i=> $"{i.Key}={i.Value}").Aggregate((a,b)=>$"{a}, {b}");
    }

    public static implicit operator LightSettings(string settings)
    {
        return new LightSettings(settings);
    }

    public static implicit operator string(LightSettings settings)
    {
        return settings.ToString();
    }
}