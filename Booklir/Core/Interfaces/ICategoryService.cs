using Booklir.Models;

namespace Booklir.Core.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<TripCategory>> GetAllAsync();
        Task<TripCategory?> GetByIdAsync(int id);
        Task CreateAsync(TripCategory entity);
        Task UpdateAsync(TripCategory entity);
        Task DeleteAsync(int id);    // could soft‑delete via IsActive=false
    }
}
