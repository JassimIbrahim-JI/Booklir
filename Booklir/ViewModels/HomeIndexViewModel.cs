using Booklir.Models;
using Booklir.ViewModels.Trip;

namespace Booklir.ViewModels
{
    public class HomeIndexViewModel
    {
        public IEnumerable<TripViewModel> FeaturedTrips { get; set; } = new List<TripViewModel>();
        public IEnumerable<Models.Destination> Destinations { get; set; } = new List<Models.Destination>();
        public IEnumerable<CategoryViewModel> Categories { get; set; } = new List<CategoryViewModel>();
    }
}
