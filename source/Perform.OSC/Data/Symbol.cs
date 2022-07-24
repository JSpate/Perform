namespace Perform.OSC.Data;

public class Symbol
{
    public readonly string Value;
		
    public Symbol(string value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Value;
    }

    public override bool Equals(object? obj)
    {
        return obj switch
        {
            Symbol symbol => Value == symbol.Value,
            string value => Value == value,
            _ => false
        };
    }

    public static bool operator ==(Symbol a, Symbol b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(Symbol a, Symbol b)
    {
        return !a.Equals(b);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}