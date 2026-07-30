using Application.Common;
using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;
using FluentValidation;

namespace Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateProductDto> _createValidator;
    private readonly IValidator<UpdateProductDto> _updateValidator;

    public ProductService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<CreateProductDto> createValidator,
        IValidator<UpdateProductDto> updateValidator)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<PagedResult<ProductDto>> GetProductsAsync(PaginationParams paginationParams, CancellationToken cancellationToken = default)
    {
        var pagedProducts = await _unitOfWork.Products.GetPagedAsync(paginationParams, cancellationToken);
        var productDtos = _mapper.Map<IEnumerable<ProductDto>>(pagedProducts.Items);

        return new PagedResult<ProductDto>(
            productDtos,
            pagedProducts.TotalCount,
            pagedProducts.PageNumber,
            pagedProducts.PageSize);
    }

    public async Task<ProductDto> GetProductByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetWithItemsByIdAsync(id, cancellationToken);
        if (product == null)
        {
            throw new NotFoundException(nameof(Product), id);
        }

        return _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto, string username, CancellationToken cancellationToken = default)
    {
        var validationResult = await _createValidator.ValidateAsync(createProductDto, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new Domain.Exceptions.ValidationException(validationResult.ToDictionary());
        }

        var product = _mapper.Map<Product>(createProductDto);
        product.CreatedBy = string.IsNullOrWhiteSpace(username) ? "System" : username;
        product.CreatedOn = DateTime.UtcNow;

        await _unitOfWork.Products.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto updateProductDto, string username, CancellationToken cancellationToken = default)
    {
        var validationResult = await _updateValidator.ValidateAsync(updateProductDto, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new Domain.Exceptions.ValidationException(validationResult.ToDictionary());
        }

        var product = await _unitOfWork.Products.GetWithItemsByIdAsync(id, cancellationToken);
        if (product == null)
        {
            throw new NotFoundException(nameof(Product), id);
        }

        _mapper.Map(updateProductDto, product);
        product.ModifiedBy = string.IsNullOrWhiteSpace(username) ? "System" : username;
        product.ModifiedOn = DateTime.UtcNow;

        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ProductDto>(product);
    }

    public async Task DeleteProductAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
        if (product == null)
        {
            throw new NotFoundException(nameof(Product), id);
        }

        _unitOfWork.Products.Delete(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
