namespace Booklir.Queries
{
    public class DestiantionQueryParameters
    {
        //Full Text Search
        public string? Search { get; set; } = null;

        public bool Status { get; set; }

        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
