using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;
using FluentValidation;

namespace Application.Services;

public class ItemService : IItemService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateItemDto> _createValidator;
    private readonly IValidator<UpdateItemDto> _updateValidator;

    public ItemService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<CreateItemDto> createValidator,
        IValidator<UpdateItemDto> updateValidator)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyList<ItemDto>> GetItemsByProductIdAsync(int productId, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken);
        if (product == null)
        {
            throw new NotFoundException(nameof(Product), productId);
        }

        var items = await _unitOfWork.Items.GetByProductIdAsync(productId, cancellationToken);
        return _mapper.Map<IReadOnlyList<ItemDto>>(items);
    }

    public async Task<ItemDto> GetItemByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var item = await _unitOfWork.Items.GetByIdAsync(id, cancellationToken);
        if (item == null)
        {
            throw new NotFoundException(nameof(Item), id);
        }

        return _mapper.Map<ItemDto>(item);
    }

    public async Task<ItemDto> CreateItemAsync(CreateItemDto createItemDto, CancellationToken cancellationToken = default)
    {
        var validationResult = await _createValidator.ValidateAsync(createItemDto, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new Domain.Exceptions.ValidationException(validationResult.ToDictionary());
        }

        var product = await _unitOfWork.Products.GetByIdAsync(createItemDto.ProductId, cancellationToken);
        if (product == null)
        {
            throw new NotFoundException(nameof(Product), createItemDto.ProductId);
        }

        var item = _mapper.Map<Item>(createItemDto);
        await _unitOfWork.Items.AddAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ItemDto>(item);
    }

    public async Task<ItemDto> UpdateItemAsync(int id, UpdateItemDto updateItemDto, CancellationToken cancellationToken = default)
    {
        var validationResult = await _updateValidator.ValidateAsync(updateItemDto, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new Domain.Exceptions.ValidationException(validationResult.ToDictionary());
        }

        var item = await _unitOfWork.Items.GetByIdAsync(id, cancellationToken);
        if (item == null)
        {
            throw new NotFoundException(nameof(Item), id);
        }

        _mapper.Map(updateItemDto, item);
        _unitOfWork.Items.Update(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ItemDto>(item);
    }

    public async Task DeleteItemAsync(int id, CancellationToken cancellationToken = default)
    {
        var item = await _unitOfWork.Items.GetByIdAsync(id, cancellationToken);
        if (item == null)
        {
            throw new NotFoundException(nameof(Item), id);
        }

        _unitOfWork.Items.Delete(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
