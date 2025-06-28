using Perform.Interfaces;
using Perform.Model;

namespace Perform.Factories;

public interface IDeviceFactory
{
    string Name { get; }

    IDevice? CreateDevice(DeviceRecord device);
}