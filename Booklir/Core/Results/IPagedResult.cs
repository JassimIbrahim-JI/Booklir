namespace Booklir.Core.Results
{
    public interface IPagedResult<T>
    {
        IReadOnlyList<T> Items { get; }
        int TotalCount { get; }
        int TotalPages { get; }
        int PageSize { get; }
        int Page { get; }
    }
}
