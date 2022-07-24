namespace Perform.OSC.Data;

public struct Midi
{
    public byte Port;
    public byte Status;
    public byte Data1;
    public byte Data2;

    public Midi(byte port, byte status, byte data1, byte data2)
    {
        Port = port;
        Status = status;
        Data1 = data1;
        Data2 = data2;
    }

    public override bool Equals(object? obj)
    {
        return obj switch
        {
            null => false,
            Midi midi => Port == midi.Port && Status == midi.Status && Data1 == midi.Data1 && Data2 == midi.Data2,
            byte[] bytes => Port == bytes[0] && Status == bytes[1] && Data1 == bytes[2] && Data2 == bytes[3],
            _ => false
        };
    }

    public static bool operator ==(Midi a, Midi b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(Midi a, Midi b)
    {
        return !a.Equals(b);
    }

    public override int GetHashCode()
    {
        return (Port << 24) + (Status << 16) + (Data1 << 8) + (Data2);
    }
}