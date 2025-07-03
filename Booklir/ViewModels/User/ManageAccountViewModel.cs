using Booklir.ViewModels.Booking;

namespace Booklir.ViewModels.User
{
    public class ManageAccountViewModel
    {
        // 1) Profile section 
        public EditProfileViewModel Profile { get; set; }

        // 2) Bookings section (paged)
        public BookingListViewModel Bookings { get; set; }
    }
}
