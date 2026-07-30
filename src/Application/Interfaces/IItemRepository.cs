using Domain.Entities;

namespace Application.Interfaces;

public interface IItemRepository : IRepository<Item>
{
    Task<IReadOnlyList<Item>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default);
}
