namespace Perform.MidiFootPedal;

internal class MidiButton(int controlCode, int index, double longPress)
{
    private readonly double _longPress = longPress < 500 ? 500 : longPress;
    private bool _isLongPressed;
    private DateTime? _pressedTime;

    public int Index { get; } = index;

    public int ControlCode { get; } = controlCode;

    public bool Down()
    {
        _pressedTime = DateTime.Now;
        return true;
    }

    public bool LongPress()
    {
        if (!_isLongPressed && _pressedTime != null)
        {
            var ms = DateTime.Now.Subtract(_pressedTime.Value).TotalMilliseconds;
            if (ms > _longPress)
            {
                _isLongPressed = true;
                return true;
            }
        }

        return false;
    }

    public bool Release()
    {
        _pressedTime = null;
        _isLongPressed = false;
        return true;
    }
}