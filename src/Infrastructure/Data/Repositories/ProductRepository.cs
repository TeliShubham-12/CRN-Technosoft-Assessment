using Application.Common;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Product?> GetWithItemsByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbContext.Products
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<PagedResult<Product>> GetPagedAsync(PaginationParams paginationParams, CancellationToken cancellationToken = default)
    {
        var query = DbContext.Products.AsNoTracking().Include(p => p.Items).AsQueryable();

        if (!string.IsNullOrWhiteSpace(paginationParams.SearchTerm))
        {
            var search = paginationParams.SearchTerm.Trim().ToLower();
            query = query.Where(p => p.ProductName.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(p => p.Id)
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Product>(items, totalCount, paginationParams.PageNumber, paginationParams.PageSize);
    }
}
