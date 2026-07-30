using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories;

public class ItemRepository : Repository<Item>, IItemRepository
{
    public ItemRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IReadOnlyList<Item>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Items
            .AsNoTracking()
            .Where(i => i.ProductId == productId)
            .ToListAsync(cancellationToken);
    }
}
