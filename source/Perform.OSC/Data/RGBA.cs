namespace Perform.OSC.Data;

public struct Rgba(byte red, byte green, byte blue, byte alpha)
{
    public byte R = red;
    public byte G = green;
    public byte B = blue;
    public byte A = alpha;

    public override bool Equals(object? obj)
    {
        return obj switch
        {
            Rgba rgba => R == rgba.R && G == rgba.G && B == rgba.B && A == rgba.A,
            byte[] bytes => R == bytes[0] && G == bytes[1] && B == bytes[2] && A == bytes[3],
            _ => false
        };
    }

    public static bool operator ==(Rgba a, Rgba b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(Rgba a, Rgba b)
    {
        return !a.Equals(b);
    }

    public override int GetHashCode()
    {
        return (R << 24) + (G << 16) + (B << 8) + (A);
    }
}