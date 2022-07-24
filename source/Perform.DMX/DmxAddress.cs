namespace Perform.DMX;

public class DmxAddress
{
    
    private DmxCommunicator? _communicator;
    private ushort? _value;

    public DmxAddress(ushort address, ushort min = 0, ushort max = 255, ushort? defaultValue = null)
    {
        if (defaultValue!=null && (defaultValue < min || defaultValue > max))
        {
            defaultValue = min;
        }

        Address = address;
        Min = min;
        Max = max;
        DefaultValue = defaultValue;
    }

    public ushort? Value
    {
        get => _value;
        set
        {
            _value = value;

            if (_value == null)
            {
                return;
            }

            if (_value < Min)
            {
                _value = Min;
            }

            if (_value > Max)
            {
                _value = Max;
            }

            if (Max > 256)
            {
                var bytes = BitConverter.GetBytes(_value.Value);
                _communicator?.SetBytes(Address - 1, new[] { bytes[1], bytes[0] });

                Console.WriteLine($"{_value} - {bytes[1]} {bytes[0]}");
            }
            else
            {
                _communicator?.SetByte(Address - 1, (byte)_value.Value);
            }
        }
    }

    public ushort Address { get; }

    public ushort Min { get; }

    public ushort Max { get; }

    public ushort? DefaultValue { get; }

    internal void Initialize(DmxCommunicator communicator)
    {
        _communicator = communicator;
        Value = DefaultValue;
    }
}