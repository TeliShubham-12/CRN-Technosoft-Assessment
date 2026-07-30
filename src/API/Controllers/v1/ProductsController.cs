using System.Security.Claims;
using Application.Common;
using Application.DTOs;
using Application.Interfaces;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IItemService _itemService;

    public ProductsController(IProductService productService, IItemService itemService)
    {
        _productService = productService;
        _itemService = itemService;
    }

    /// <summary>
    /// Get paginated list of products.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetProducts([FromQuery] PaginationParams paginationParams, CancellationToken cancellationToken)
    {
        var result = await _productService.GetProductsAsync(paginationParams, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get specific product by ID along with its items.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetProductById(int id, CancellationToken cancellationToken)
    {
        var product = await _productService.GetProductByIdAsync(id, cancellationToken);
        return Ok(product);
    }

    /// <summary>
    /// Get all items belonging to a product.
    /// </summary>
    [HttpGet("{id:int}/items")]
    [ProducesResponseType(typeof(IReadOnlyList<ItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ItemDto>>> GetProductItems(int id, CancellationToken cancellationToken)
    {
        var items = await _itemService.GetItemsByProductIdAsync(id, cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Create a new product. (Requires Authentication)
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductDto createProductDto, CancellationToken cancellationToken)
    {
        var username = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "System";
        var product = await _productService.CreateProductAsync(createProductDto, username, cancellationToken);
        return CreatedAtAction(nameof(GetProductById), new { id = product.Id, version = "1.0" }, product);
    }

    /// <summary>
    /// Update an existing product. (Requires Authentication)
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> UpdateProduct(int id, [FromBody] UpdateProductDto updateProductDto, CancellationToken cancellationToken)
    {
        var username = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "System";
        var product = await _productService.UpdateProductAsync(id, updateProductDto, username, cancellationToken);
        return Ok(product);
    }

    /// <summary>
    /// Delete a product. (Requires Authentication)
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(int id, CancellationToken cancellationToken)
    {
        await _productService.DeleteProductAsync(id, cancellationToken);
        return NoContent();
    }
}
