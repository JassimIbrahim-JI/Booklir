using Booklir.Core.Results;
using Booklir.Models;
using Booklir.Queries;

namespace Booklir.ViewModels.Trip
{
    public class TripSearchViewModel
    {
        public IPagedResult<TripViewModel> Results { get; set; }
        public TripSearchFilters Filters { get; set; }
        public IEnumerable<Models.Destination> Destinations { get; set; }
    }
}
