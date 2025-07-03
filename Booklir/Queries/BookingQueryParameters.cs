using Booklir.enums;

namespace Booklir.Queries
{
    public class BookingQueryParameters
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? TripId { get; set; }
        public string? UserEmail { get; set; }
        public bool ShowDeleted { get; set; } = false;
        public BookingStatus? Status { get; set; }
        public PaymentMethod? PaymentMethod {  get; set; } 
        public PaymentStatus? PaymentStatus { get; set; }
        //Full Text Search
        public string? Search { get; set; } = null;

        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

    }
}
