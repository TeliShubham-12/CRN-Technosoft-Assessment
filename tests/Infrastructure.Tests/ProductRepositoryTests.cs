using Application.Common;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Infrastructure.Tests;

public class ProductRepositoryTests
{
    private ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task GetPagedAsync_AppliesPaginationAndSearchFilter()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        dbContext.Products.AddRange(
            new Product { Id = 1, ProductName = "Apple iPhone", CreatedBy = "Admin" },
            new Product { Id = 2, ProductName = "Samsung Galaxy", CreatedBy = "Admin" },
            new Product { Id = 3, ProductName = "Apple iPad", CreatedBy = "Admin" }
        );
        await dbContext.SaveChangesAsync();

        var repo = new ProductRepository(dbContext);
        var paramsObj = new PaginationParams { PageNumber = 1, PageSize = 10, SearchTerm = "Apple" };

        // Act
        var result = await repo.GetPagedAsync(paramsObj);

        // Assert
        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, item => Assert.Contains("Apple", item.ProductName));
    }

    [Fact]
    public async Task GetWithItemsByIdAsync_IncludesRelatedItems()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        var product = new Product
        {
            Id = 1,
            ProductName = "Desk Laptop",
            CreatedBy = "Admin",
            Items = new List<Item>
            {
                new Item { Id = 10, Quantity = 5 },
                new Item { Id = 11, Quantity = 2 }
            }
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var repo = new ProductRepository(dbContext);

        // Act
        var result = await repo.GetWithItemsByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
    }
}
