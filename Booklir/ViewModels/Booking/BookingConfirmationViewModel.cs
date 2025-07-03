namespace Booklir.ViewModels.Booking
{
    public class BookingConfirmationViewModel
    {
        public string CustomerName { get; set; }
        public string CompanyName { get; set; }
        public string TripTitle { get; set; }
        public DateTime BookingDate { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
