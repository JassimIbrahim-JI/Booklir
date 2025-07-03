namespace Booklir.Queries
{
    public class TripSearchFilters
    {
        public string Query { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Duration { get; set; }
        public int? DestinationId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int? CategoryId { get; set; }
    }
}
