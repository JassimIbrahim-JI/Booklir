using Booklir.Queries;

namespace Booklir.ViewModels.Trip
{
    public class TripListViewModel
    {
        public IEnumerable<TripViewModel> Trips { get; set; } = new List<TripViewModel>();
        public int TotalTrips { get; set; }
        public TripQueryParameters Parameters { get; set; }
    }
}
