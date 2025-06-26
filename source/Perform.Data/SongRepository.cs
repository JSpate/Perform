using Microsoft.Data.Sqlite;
using Perform.Interfaces;
using Perform.Model;

namespace Perform.Data;

public class SongRepository(SqliteConnection connection) : ISongRepository
{
    public async Task<Song?> GetByIdAsync(string id)
    {
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT Id, Name, Description, TimeSignature, Bpm, Script, Lyrics FROM Song WHERE Id = $id";
        cmd.Parameters.AddWithValue("$id", id);
        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Song
            {
                Name = reader.GetString(1),
                Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                TimeSignature = reader.IsDBNull(3) ? null : reader.GetString(3),
                BPM = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                // Lyrics and Consoles mapping omitted for brevity
            };
        }
        return null;
    }

    public async Task<IEnumerable<Song>> GetAllAsync()
    {
        var result = new List<Song>();
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT Id, Name, Description, TimeSignature, Bpm, Script, Lyrics FROM Song";
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new Song
            {
                Name = reader.GetString(1),
                Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                TimeSignature = reader.IsDBNull(3) ? null : reader.GetString(3),
                BPM = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                // Lyrics and Consoles mapping omitted for brevity
            });
        }
        return result;
    }

    public async Task AddAsync(Song song)
    {
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
                INSERT INTO Song (Id, Name, Description, TimeSignature, Bpm, Script, Lyrics)
                VALUES ($id, $name, $description, $timeSignature, $bpm, $script, $lyrics)";
        cmd.Parameters.AddWithValue("$id", song.Name);
        cmd.Parameters.AddWithValue("$name", song.Name);
        cmd.Parameters.AddWithValue("$description", song.Description ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$timeSignature", song.TimeSignature ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$bpm", song.BPM ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$script", DBNull.Value); // Adjust as needed
        cmd.Parameters.AddWithValue("$lyrics", DBNull.Value); // Adjust as needed
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdateAsync(Song song)
    {
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
                UPDATE Song
                SET Name = $name, Description = $description, TimeSignature = $timeSignature, Bpm = $bpm, Script = $script, Lyrics = $lyrics
                WHERE Id = $id";
        cmd.Parameters.AddWithValue("$id", song.Name);
        cmd.Parameters.AddWithValue("$name", song.Name);
        cmd.Parameters.AddWithValue("$description", song.Description ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$timeSignature", song.TimeSignature ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$bpm", song.BPM ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$script", DBNull.Value); // Adjust as needed
        cmd.Parameters.AddWithValue("$lyrics", DBNull.Value); // Adjust as needed
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(string id)
    {
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM Song WHERE Id = $id";
        cmd.Parameters.AddWithValue("$id", id);
        await cmd.ExecuteNonQueryAsync();
    }
}