using Application.Common;
using Domain.Entities;

namespace Application.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetWithItemsByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PagedResult<Product>> GetPagedAsync(PaginationParams paginationParams, CancellationToken cancellationToken = default);
}
