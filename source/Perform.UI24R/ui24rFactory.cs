using Perform.Factories;
using Perform.Model;

namespace Perform.UI24R;

public class UI24RFactory : IDeviceFactory
{
    public string Name => "ui24r";

    public IDevice? CreateDevice(DeviceRecord device)
    {
        return null;
    }
}