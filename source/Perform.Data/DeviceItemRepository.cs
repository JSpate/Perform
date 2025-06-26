using Microsoft.Data.Sqlite;
using Perform.Interfaces;
using Perform.Model;

namespace Perform.Data;

public class DeviceItemRepository(SqliteConnection connection) : IDeviceItemRepository
{
    public async Task<IEnumerable<DeviceItem>> GetByDeviceIdAsync(string deviceId)
    {
        var result = new List<DeviceItem>();
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT DeviceId, DeviceItem, Config FROM DeviceItem WHERE DeviceId = $deviceId";
        cmd.Parameters.AddWithValue("$deviceId", deviceId);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new DeviceItem
            (
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2)
            ));
        }
        return result;
    }

    public async Task<DeviceItem?> GetAsync(string deviceId, string deviceItem)
    {
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT DeviceId, DeviceItem, Config FROM DeviceItem WHERE DeviceId = $deviceId AND DeviceItem = $deviceItem";
        cmd.Parameters.AddWithValue("$deviceId", deviceId);
        cmd.Parameters.AddWithValue("$deviceItem", deviceItem);
        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new DeviceItem
            (
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2)
            );
        }
        return null;
    }

    public async Task AddAsync(DeviceItem item)
    {
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
                INSERT INTO DeviceItem (DeviceId, DeviceItem, Config)
                VALUES ($deviceId, $deviceItem, $config)";
        cmd.Parameters.AddWithValue("$deviceId", item.DeviceId);
        cmd.Parameters.AddWithValue("$deviceItem", item.DeviceItemName);
        cmd.Parameters.AddWithValue("$config", item.Config);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdateAsync(DeviceItem item)
    {
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
                UPDATE DeviceItem
                SET Config = $config
                WHERE DeviceId = $deviceId AND DeviceItem = $deviceItem";
        cmd.Parameters.AddWithValue("$deviceId", item.DeviceId);
        cmd.Parameters.AddWithValue("$deviceItem", item.DeviceItemName);
        cmd.Parameters.AddWithValue("$config", item.Config);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(string deviceId, string deviceItem)
    {
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM DeviceItem WHERE DeviceId = $deviceId AND DeviceItem = $deviceItem";
        cmd.Parameters.AddWithValue("$deviceId", deviceId);
        cmd.Parameters.AddWithValue("$deviceItem", deviceItem);
        await cmd.ExecuteNonQueryAsync();
    }
}