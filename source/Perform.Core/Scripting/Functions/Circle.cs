using Perform.Interfaces;

namespace Perform.Scripting.Functions;

public class Circle(double radius, double centerPan, double centerTilt, double speed)
    : IDeviceScriptFunction
{
    public void Initialize(ShowScript showScript, params string[] devices)
    {
        throw new NotImplementedException();
    }

    public void Loop()
    {
        throw new NotImplementedException();
    }

    public bool IsFinished => false;
}