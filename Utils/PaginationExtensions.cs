using Microsoft.EntityFrameworkCore;

namespace WebBanMayTinh.Utils
{
    public static class PaginationExtensions
    {
        public static async Task<PaginationResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        int page,
        int pageSize,
        int maxPageDisplay = 3)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            int totalItems = await query.CountAsync();

            var items = await query
                .Skip(Math.Max(page - 1, 0) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginationResult<T>(
                items,
                totalItems,
                page,
                pageSize,
                maxPageDisplay
            );
        }
    }
}
