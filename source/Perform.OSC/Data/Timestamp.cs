namespace Perform.OSC.Data;

public struct Timestamp
{
    public ulong Tag;
        
    private Timestamp(ulong value)
    {
        Tag = value;
    }

    public override bool Equals(object? obj)
    {
        return obj switch
        {
            Timestamp timestamp => Tag == timestamp.Tag,
            ulong @ulong => Tag == @ulong,
            _ => false
        };
    }

    public static bool operator ==(Timestamp a, Timestamp b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(Timestamp a, Timestamp b)
    {
        return !a.Equals(b);
    }

    public override int GetHashCode()
    {
        return (int)( ((uint)(Tag >> 32) + (uint)(Tag & 0x00000000FFFFFFFF)) / 2);
    }

    public static implicit operator DateTime(Timestamp timestamp)
    {
        if (timestamp.Tag == 1)
            return DateTime.Now;

        var seconds = (uint)(timestamp.Tag >> 32);
        var time = DateTime.Parse("1900-01-01 00:00:00");
        time = time.AddSeconds(seconds);
        var fraction = CalculateToFraction(timestamp.Tag);
        time = time.AddSeconds(fraction);
        return time;
    }

    public static implicit operator Timestamp(DateTime dateTime)
    {
        ulong seconds = (ulong)(dateTime - DateTime.Parse("1900-01-01 00:00:00.000")).TotalSeconds;
        ulong fraction = (ulong)(0xFFFFFFFF * ((double)dateTime.Millisecond / 1000));
            
        return new Timestamp((seconds << 32) + fraction);
    }

    public static implicit operator Timestamp(ulong tag)
    {
        return new Timestamp(tag);
    }

    public static implicit operator ulong(Timestamp timestamp)
    {
        return timestamp.Tag;
    }

    private static double CalculateToFraction(ulong val)
    {
        if (val == 1)
            return 0.0;

        var seconds = (uint)(val & 0x00000000FFFFFFFF);
        var fraction = (double)seconds / 0xFFFFFFFF;
        return fraction;
    }
}