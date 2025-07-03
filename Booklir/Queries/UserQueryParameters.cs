namespace Booklir.Queries
{
    public class UserQueryParameters
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; } = null;
        public string? Role { get; set; }
        public bool? IsActive { get; set; }
    }
}
