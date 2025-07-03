using Booklir.enums;

namespace Booklir.ViewModels.Booking
{
    public class BookingViewModel
    {
        public int Id { get; set; }
        public int TripId { get; set; }
        public string UserName { get; set; }
        public string TripTitle { get; set; }
        public DateTime BookingDate { get; set; }
        public BookingStatus Status { get; set; }
        public decimal TotalPrice { get; set; }

        public Models.Trip Trip { get; set; }
        public string PrimaryImage { get; set; }
        public int Quantity { get; set; }

        public PaymentStatus? PaymentStatus { get; set; } // Nullable to allow for bookings that haven't been paid yet
        public PaymentMethod? PaymentMethod { get; set; } // Nullable to allow for bookings that haven't been paid yet

        public bool IsDeleted { get; set; }
        public DateTime DeletedAt { get; set; } // Used for soft delete functionality
        public string DeletedByAdminId { get; set; } // Used for soft delete functionality

        //This ViewModel is used for displaying or creating a Booking

    }
}
