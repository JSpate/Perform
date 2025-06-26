//namespace Perform.Model.Console;

//public readonly struct LevelValue
//{
//    private const int MinInt = 0;
//    private const int MaxInt = 250;

//    private const float MinFloat = 0;
//    private const float MaxFloat = 1f;

//    private const float IntStep = MaxFloat / MaxInt;

//    private LevelValue(int value)
//    {
//        Value = value;
//    }

//    public int Value { get; }

//    public bool Equals(LevelValue other)
//    {
//        return Value == other.Value;
//    }

//    public override bool Equals(object? obj)
//    {
//        return obj is LevelValue other && Equals(other);
//    }

//    public override int GetHashCode()
//    {
//        return Value;
//    }

//    public static implicit operator int(LevelValue value)
//    {
//        return value.Value;
//    }

//    public static implicit operator float(LevelValue value)
//    {
//        return IntStep * value.Value;
//    }

//    public static implicit operator LevelValue(float value)
//    {
//        if (value < MinFloat)
//        {
//            value = MinFloat;
//        }

//        if (value > MaxFloat)
//        {
//            value = MaxFloat;
//        }

//        return new LevelValue((int)(MaxInt * value));
//    }

//    public static implicit operator LevelValue(double value)
//    {
//        if (value < MinFloat)
//        {
//            value = MinFloat;
//        }

//        if (value > MaxFloat)
//        {
//            value = MaxFloat;
//        }

//        return new LevelValue((int)(MaxInt * value));
//    }

//    public static bool operator ==(LevelValue lhs, LevelValue rhs)
//    {
//        return lhs.Value == rhs.Value;
//    }

//    public static bool operator !=(LevelValue lhs, LevelValue rhs)
//    {
//        return lhs.Value != rhs.Value;
//    }

//    public static LevelValue operator +(LevelValue lhs, LevelValue rhs)
//    {
//        var value = lhs.Value + rhs.Value;

//        if (value > MaxInt)
//        {
//            value = MaxInt;
//        }

//        return new LevelValue(value);
//    }

//    public static LevelValue operator -(LevelValue lhs, LevelValue rhs)
//    {
//        var value = lhs.Value - rhs.Value;

//        if (value < MinInt)
//        {
//            value = MinInt;
//        }

//        return new LevelValue(value);
//    }
//}