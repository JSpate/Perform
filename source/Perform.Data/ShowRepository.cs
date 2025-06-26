using Microsoft.Data.Sqlite;
using Perform.Interfaces;
using Perform.Model;
using System.Text.Json;

namespace Perform.Data;

public class ShowRepository(SqliteConnection connection) : IShowRepository
{
    public async Task<Show?> GetByIdAsync(string id)
    {
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT Data FROM Show WHERE Id = $id";
        cmd.Parameters.AddWithValue("$id", id);
        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var json = reader.GetString(0);
            return JsonSerializer.Deserialize<Show>(json, JsonOptions());
        }
        return null;
    }

    public async Task<IEnumerable<string>> ListAsync()
    {
        var result = new List<string>();
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT Id FROM Show";
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(reader.GetString(0));
        }
        return result;
    }

    public async Task AddAsync(Show show)
    {
        var json = JsonSerializer.Serialize(show, JsonOptions());
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Show (Id, Data)
            VALUES ($id, $data)";
        cmd.Parameters.AddWithValue("$id", show.Name);
        cmd.Parameters.AddWithValue("$data", json);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdateAsync(Show show)
    {
        var json = JsonSerializer.Serialize(show, JsonOptions());
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            UPDATE Show
            SET Data = $data
            WHERE Id = $id";
        cmd.Parameters.AddWithValue("$id", show.Name);
        cmd.Parameters.AddWithValue("$data", json);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(string id)
    {
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM Show WHERE Id = $id";
        cmd.Parameters.AddWithValue("$id", id);
        await cmd.ExecuteNonQueryAsync();
    }

    private static JsonSerializerOptions JsonOptions() => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };
}