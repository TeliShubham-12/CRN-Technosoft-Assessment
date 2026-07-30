using Application.Interfaces;
using Infrastructure.Data.Repositories;

namespace Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _dbContext;
    private IProductRepository? _productRepository;
    private IItemRepository? _itemRepository;
    private IUserRepository? _userRepository;

    public UnitOfWork(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IProductRepository Products => _productRepository ??= new ProductRepository(_dbContext);
    public IItemRepository Items => _itemRepository ??= new ItemRepository(_dbContext);
    public IUserRepository Users => _userRepository ??= new UserRepository(_dbContext);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
