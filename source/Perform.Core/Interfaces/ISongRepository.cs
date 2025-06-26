using Perform.Model;

namespace Perform.Interfaces;

public interface ISongRepository
{
    Task<Song?> GetByIdAsync(string id);
    Task<IEnumerable<Song>> GetAllAsync();
    Task AddAsync(Song song);
    Task UpdateAsync(Song song);
    Task DeleteAsync(string id);
}