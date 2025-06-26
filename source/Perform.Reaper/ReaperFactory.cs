using System.Text.Json;
using Perform.Factories;
using Perform.Model;

namespace Perform.Reaper;

public class ReaperFactory : IDeviceFactory
{
    public string Name => "reaper";

    public IDevice CreateDevice(DeviceRecord device)
    {
        var config = JsonSerializer.Deserialize<OscConfig>(JsonSerializer.Serialize(device.Settings));
        if(config == null)
        {
            throw new InvalidOperationException("Invalid config for reaper console");
        }

        return new Reaper("reaper", config);
    }
}