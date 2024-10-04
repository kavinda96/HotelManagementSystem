using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Services
{
    

    public class PaginationService
    {
        public async Task<Pagination<T>> GetPaginatedListAsync<T>(
            IQueryable<T> source, int pageIndex, int pageSize)
        {
            var totalRecords = await source.CountAsync();
            var items = await source.Skip((pageIndex - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync();
            return new Pagination<T>(items, totalRecords, pageSize, pageIndex);
        }
    }
}
