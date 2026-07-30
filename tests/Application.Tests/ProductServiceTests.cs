using Application.Common;
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

public class ProductServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IValidator<CreateProductDto>> _createValidatorMock;
    private readonly Mock<IValidator<UpdateProductDto>> _updateValidatorMock;
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _productRepoMock = new Mock<IProductRepository>();
        _unitOfWorkMock.Setup(x => x.Products).Returns(_productRepoMock.Object);

        _mapperMock = new Mock<IMapper>();
        _createValidatorMock = new Mock<IValidator<CreateProductDto>>();
        _updateValidatorMock = new Mock<IValidator<UpdateProductDto>>();

        _productService = new ProductService(
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _createValidatorMock.Object,
            _updateValidatorMock.Object);
    }

    [Fact]
    public async Task GetProductByIdAsync_ExistingId_ReturnsProductDto()
    {
        // Arrange
        var productId = 1;
        var product = new Product { Id = productId, ProductName = "Test Product", CreatedBy = "Admin" };
        var productDto = new ProductDto { Id = productId, ProductName = "Test Product", CreatedBy = "Admin" };

        _productRepoMock.Setup(x => x.GetWithItemsByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _mapperMock.Setup(x => x.Map<ProductDto>(product)).Returns(productDto);

        // Act
        var result = await _productService.GetProductByIdAsync(productId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(productId);
        result.ProductName.Should().Be("Test Product");
    }

    [Fact]
    public async Task GetProductByIdAsync_NonExistingId_ThrowsNotFoundException()
    {
        // Arrange
        var productId = 99;
        _productRepoMock.Setup(x => x.GetWithItemsByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _productService.GetProductByIdAsync(productId));
    }

    [Fact]
    public async Task CreateProductAsync_ValidDto_ReturnsCreatedProductDto()
    {
        // Arrange
        var createDto = new CreateProductDto { ProductName = "New Laptop" };
        var productEntity = new Product { Id = 1, ProductName = "New Laptop", CreatedBy = "Tester" };
        var productDto = new ProductDto { Id = 1, ProductName = "New Laptop", CreatedBy = "Tester" };

        _createValidatorMock.Setup(x => x.ValidateAsync(createDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _mapperMock.Setup(x => x.Map<Product>(createDto)).Returns(productEntity);
        _mapperMock.Setup(x => x.Map<ProductDto>(productEntity)).Returns(productDto);

        // Act
        var result = await _productService.CreateProductAsync(createDto, "Tester");

        // Assert
        result.Should().NotBeNull();
        result.ProductName.Should().Be("New Laptop");
        _productRepoMock.Verify(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
