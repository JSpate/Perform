//using System.Text.Json;
//using Microsoft.Data.Sqlite;
//using Perform.Interfaces;
//using Perform.Model;

//namespace Perform.Data;

//public class DeviceRepository(SqliteConnection connection) : IDeviceRepository
//{
//    public async Task<DeviceRecord?> GetByIdAsync(string id)
//    {
//        await using var cmd = connection.CreateCommand();
//        cmd.CommandText = "SELECT Id, Type, Description, DefaultSettings FROM Device WHERE Id = $id";
//        cmd.Parameters.AddWithValue("$id", id);
//        await using var reader = await cmd.ExecuteReaderAsync();
//        if (await reader.ReadAsync())
//        {
//            return new DeviceRecord
//            (
//                reader.GetString(0),
//                reader.GetString(1),
//                reader.GetString(2),
//                JsonElement.ParseValue(/*reader.GetString(3)*/)
//            );
//        }
//        return null;
//    }

//    public async Task<IEnumerable<DeviceRecord>> GetAllAsync()
//    {
//        var result = new List<DeviceRecord>();
//        await using var cmd = connection.CreateCommand();
//        cmd.CommandText = "SELECT Id, Type, Description, DefaultSettings FROM Device";
//        await using var reader = await cmd.ExecuteReaderAsync();
//        while (await reader.ReadAsync())
//        {
//            result.Add(new DeviceRecord
//            (
//                reader.GetString(0),
//                reader.GetString(1),
//                reader.GetString(2),
//                reader.GetString(3)
//            ));
//        }
//        return result;
//    }

//    public async Task AddAsync(DeviceRecord device)
//    {
//        await using var cmd = connection.CreateCommand();
//        cmd.CommandText = @"
//                INSERT INTO Device (Id, Type, Description, DefaultSettings)
//                VALUES ($id, $type, $description, $settings)";
//        cmd.Parameters.AddWithValue("$id", device.Id);
//        cmd.Parameters.AddWithValue("$type", device.Type);
//        cmd.Parameters.AddWithValue("$description", device.Description);
//        cmd.Parameters.AddWithValue("$settings", device.Settings);
//        await cmd.ExecuteNonQueryAsync();
//    }

//    public async Task UpdateAsync(DeviceRecord device)
//    {
//        await using var cmd = connection.CreateCommand();
//        cmd.CommandText = @"
//                UPDATE Device
//                SET Type = $type, Description = $description, DefaultSettings = $settings
//                WHERE Id = $id";
//        cmd.Parameters.AddWithValue("$id", device.Id);
//        cmd.Parameters.AddWithValue("$type", device.Type);
//        cmd.Parameters.AddWithValue("$description", device.Description);
//        cmd.Parameters.AddWithValue("$settings", device.Settings);
//        await cmd.ExecuteNonQueryAsync();
//    }

//    public async Task DeleteAsync(string id)
//    {
//        await using var cmd = connection.CreateCommand();
//        cmd.CommandText = "DELETE FROM Device WHERE Id = $id";
//        cmd.Parameters.AddWithValue("$id", id);
//        await cmd.ExecuteNonQueryAsync();
//    }
//}