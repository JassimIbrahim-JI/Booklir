using Booklir.Models;

namespace Booklir.Core.Results
{
    public class PagedResult<T> : IPagedResult<T>
    {

        public IReadOnlyList<T> Items { get; }
        public int TotalCount { get; }
        public int PageSize { get; }
        public int Page { get; }

        public PagedResult(IEnumerable<T> items, int totalCount, int pageSize, int page)
        {
            Items = items.ToList().AsReadOnly();
            TotalCount = totalCount;
            PageSize = pageSize;
            Page = page;
        }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}
