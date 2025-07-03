using Booklir.Queries;

namespace Booklir.ViewModels.Booking
{
    public class BookingListViewModel
    {
        public IEnumerable<BookingViewModel> Items { get; set; }
        public int TotalCount { get; set; }
        public BookingQueryParameters Parameters { get; set; }
    }
}
