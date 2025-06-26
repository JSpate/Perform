using Perform.Model;

namespace Perform.Interfaces;

public interface IDeviceItemRepository
{
    Task<IEnumerable<DeviceItem>> GetByDeviceIdAsync(string deviceId);
    Task<DeviceItem?> GetAsync(string deviceId, string deviceItem);
    Task AddAsync(DeviceItem item);
    Task UpdateAsync(DeviceItem item);
    Task DeleteAsync(string deviceId, string deviceItem);
}