using Perform.Model;

namespace Perform.Interfaces;

public interface IShowRepository
{
    Task<Show?> GetByIdAsync(string id);
    
    Task<IEnumerable<string>> ListAsync();

    Task AddAsync(Show show);

    Task UpdateAsync(Show show);

    Task DeleteAsync(string id);
}