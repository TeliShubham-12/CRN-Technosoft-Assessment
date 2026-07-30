using Application.Common;
using Application.DTOs;

namespace Application.Interfaces;

public interface IProductService
{
    Task<PagedResult<ProductDto>> GetProductsAsync(PaginationParams paginationParams, CancellationToken cancellationToken = default);
    Task<ProductDto> GetProductByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto, string username, CancellationToken cancellationToken = default);
    Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto updateProductDto, string username, CancellationToken cancellationToken = default);
    Task DeleteProductAsync(int id, CancellationToken cancellationToken = default);
}
