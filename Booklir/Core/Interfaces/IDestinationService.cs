using Booklir.Core.Results;
using Booklir.Models;
using Booklir.Queries;
using Booklir.ViewModels.Destination;
using System.Collections;

namespace Booklir.Core.Interfaces
{
    public interface IDestinationService
    {
        Task<IPagedResult<Destination>> GetAllAsync(DestiantionQueryParameters p);
        Task<IEnumerable<Destination>> GetPopularDestinationAsync(int count);
        Task<Destination?> GetByIdAsync(int id);
        Task<bool> CreateAsync(Destination destination, IFormFile image);
        Task<bool> UpdateAsync(Destination destination, IFormFile image);
        Task<bool> ToogleActiveAsync(int id, bool status);
    }
}
