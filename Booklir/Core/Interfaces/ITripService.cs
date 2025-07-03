using Booklir.Core.Results;
using Booklir.Models;
using Booklir.Queries;

namespace Booklir.Core.Interfaces
{
    public interface ITripService
    {
        Task<bool> CreateTripAsync(Trip trip, List<IFormFile> imageFiles,string adminId);
        Task<bool> UpdateTripAsync(Trip existingTrip);
        Task<bool> DeleteTripAsync(int tripId);
        Task<Trip?> GetTripByIdAsync(int tripId);
        Task<IEnumerable<Trip>> GetAvailableTripsAsync();
        Task<IEnumerable<Trip>> GetTripsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Trip>> GetFeaturedTripsAsync();
        Task<IPagedResult<Trip>> SearchHomeTripAsync(TripSearchFilters filter);
        Task<AdminStatistics> GetAdminStatisticsAsync();

        Task<bool> SoftDeleteAsync(int tripId, string adminId);
        Task<bool> RestoreAsync(int tripId, string adminId);
        Task<Stream> ExportCsvAsync(TripQueryParameters p);
        Task<IPagedResult<Trip>> GetTripsAsync(TripQueryParameters p);
    }
}
