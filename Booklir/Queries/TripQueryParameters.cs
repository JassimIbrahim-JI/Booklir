namespace Booklir.Queries
{
    public class TripQueryParameters
    {
        // Filterin
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Status { get; set; } // Active or Deactive

        public string ShowDeleted { get; set; } // "true", "false", or null

        // Full-text search
        public string? SearchTerm { get; set; } = null;

        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
