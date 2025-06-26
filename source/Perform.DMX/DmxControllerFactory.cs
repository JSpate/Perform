using Perform.Factories;
using Perform.Model;

namespace Perform.DMX
{
    public class DmxControllerFactory : IDeviceFactory
    {
        public string Name => "dmxController";

        public IDevice? CreateDevice(DeviceRecord device)
        {
            return null;
        }
    }
}
