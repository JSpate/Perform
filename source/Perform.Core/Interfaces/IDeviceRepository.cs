using Perform.Model;

namespace Perform.Interfaces;

public interface IDeviceRepository
{
    Task<DeviceRecord?> GetByIdAsync(string id);
    Task<IEnumerable<DeviceRecord>> GetAllAsync();
    Task AddAsync(DeviceRecord device);
    Task UpdateAsync(DeviceRecord device);
    Task DeleteAsync(string id);
}