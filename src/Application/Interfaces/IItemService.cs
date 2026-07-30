using Application.DTOs;

namespace Application.Interfaces;

public interface IItemService
{
    Task<IReadOnlyList<ItemDto>> GetItemsByProductIdAsync(int productId, CancellationToken cancellationToken = default);
    Task<ItemDto> GetItemByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ItemDto> CreateItemAsync(CreateItemDto createItemDto, CancellationToken cancellationToken = default);
    Task<ItemDto> UpdateItemAsync(int id, UpdateItemDto updateItemDto, CancellationToken cancellationToken = default);
    Task DeleteItemAsync(int id, CancellationToken cancellationToken = default);
}
