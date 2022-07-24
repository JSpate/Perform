namespace Perform.Model;

internal class Button
{
    private readonly double _longPress;
    private bool _isLongPressed;
    private DateTime? _pressedTime;

    public Button(int keyCode, int index, double longPress)
    {
        _longPress = longPress < 500 ? 500 : longPress;
        Index = index;
        KeyCode = keyCode;
    }

    public int Index { get; }

    public int KeyCode { get; }

    public bool Press()
    {
        if (_pressedTime != null)
        {
            return false;
        }

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