using crudcomdb.Models;

namespace crudcomdb.Interfaces
{
    public interface IInteresseRepository
    {
        Task AddAsync(InteresseAdocao interesse);
        Task<IEnumerable<InteresseAdocao>> GetAllAsync();
    }
}