using Application.DTOs;
using Application.Interfaces;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Xunit;

namespace Application.Tests;

public class ItemServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly Mock<IItemRepository> _itemRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IValidator<CreateItemDto>> _createValidatorMock;
    private readonly Mock<IValidator<UpdateItemDto>> _updateValidatorMock;
    private readonly ItemService _itemService;

    public ItemServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _productRepoMock = new Mock<IProductRepository>();
        _itemRepoMock = new Mock<IItemRepository>();

        _unitOfWorkMock.Setup(x => x.Products).Returns(_productRepoMock.Object);
        _unitOfWorkMock.Setup(x => x.Items).Returns(_itemRepoMock.Object);

        _mapperMock = new Mock<IMapper>();
        _createValidatorMock = new Mock<IValidator<CreateItemDto>>();
        _updateValidatorMock = new Mock<IValidator<UpdateItemDto>>();

        _itemService = new ItemService(
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _createValidatorMock.Object,
            _updateValidatorMock.Object);
    }

    [Fact]
    public async Task CreateItemAsync_ValidDto_CreatesAndReturnsItemDto()
    {
        // Arrange
        var createDto = new CreateItemDto { ProductId = 1, Quantity = 10 };
        var product = new Product { Id = 1, ProductName = "Keyboard" };
        var itemEntity = new Item { Id = 101, ProductId = 1, Quantity = 10 };
        var itemDto = new ItemDto { Id = 101, ProductId = 1, Quantity = 10 };

        _createValidatorMock.Setup(x => x.ValidateAsync(createDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _productRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _mapperMock.Setup(x => x.Map<Item>(createDto)).Returns(itemEntity);
        _mapperMock.Setup(x => x.Map<ItemDto>(itemEntity)).Returns(itemDto);

        // Act
        var result = await _itemService.CreateItemAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Quantity.Should().Be(10);
        _itemRepoMock.Verify(x => x.AddAsync(itemEntity, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateItemAsync_NonExistingProductId_ThrowsNotFoundException()
    {
        // Arrange
        var createDto = new CreateItemDto { ProductId = 99, Quantity = 5 };

        _createValidatorMock.Setup(x => x.ValidateAsync(createDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _productRepoMock.Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _itemService.CreateItemAsync(createDto));
    }
}
