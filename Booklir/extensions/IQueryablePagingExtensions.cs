using Booklir.Core.Results;
using Microsoft.EntityFrameworkCore;

namespace Booklir.extensions
{
    public static class IQueryablePagingExtensions
    {
        public static async Task<IPagedResult<T>> ToPagedResultAsync<T>(this IQueryable<T> source, int page, int pageSize)
        {
            var totalCount = await source.CountAsync();
            var items = await source.Skip((page-1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResult<T>(items, totalCount, pageSize, page);

        }
    }
}
