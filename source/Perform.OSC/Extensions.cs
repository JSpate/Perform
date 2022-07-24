using Perform.OSC.Data;

namespace Perform.OSC;

internal static class Extensions
{
    public static int FirstIndexAfter<T>(this IEnumerable<T> items, int start, Func<T, bool> predicate)
    {
        if (items == null) throw new ArgumentNullException(nameof(items));
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));

        var itemArray = items.ToArray();
        if (!itemArray.Any()) throw new ArgumentOutOfRangeException(nameof(start));

        var retVal = 0;
        foreach (var item in itemArray)
        {
            if (retVal >= start && predicate(item)) return retVal;
            retVal++;
        }
        return -1;
    }

    public static T[] SubArray<T>(this T[] data, int index, int length)
    {
        var result = new T[length];
        Array.Copy(data, index, result, 0, length);
        return result;
    }
		
    public static int AlignedStringLength(this string val)
    {
        var len = val.Length + (4 - val.Length % 4);
        if (len <= val.Length) len += 4;

        return len;
    }

    public static int AlignedStringLength(this Address address)
    {
        return AlignedStringLength((string)address);
    }
}