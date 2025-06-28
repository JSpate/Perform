using Perform.Interfaces;
using Perform.Model;

namespace Perform.Factories;

public class DeviceFactory(IEnumerable<IDeviceFactory> factories)
{
    public IDevice? CreateDevice(DeviceRecord device)
    {
        if(device == null)
        {
            throw new ArgumentNullException(nameof(device));
        }

        var factory = factories.FirstOrDefault(f => f.Name == device.Type);
        return factory == null 
            ? throw new InvalidOperationException($"No factory found for device {device.Type}") 
            : factory.CreateDevice(device);
    }
}