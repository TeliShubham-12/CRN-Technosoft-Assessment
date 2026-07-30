using Application.DTOs;
using Application.Interfaces;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly IItemService _itemService;

    public ItemsController(IItemService itemService)
    {
        _itemService = itemService;
    }

    /// <summary>
    /// Get specific item by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ItemDto>> GetItemById(int id, CancellationToken cancellationToken)
    {
        var item = await _itemService.GetItemByIdAsync(id, cancellationToken);
        return Ok(item);
    }

    /// <summary>
    /// Create a new item for a product. (Requires Authentication)
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ItemDto>> CreateItem([FromBody] CreateItemDto createItemDto, CancellationToken cancellationToken)
    {
        var item = await _itemService.CreateItemAsync(createItemDto, cancellationToken);
        return CreatedAtAction(nameof(GetItemById), new { id = item.Id, version = "1.0" }, item);
    }

    /// <summary>
    /// Update an existing item's quantity. (Requires Authentication)
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(ItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ItemDto>> UpdateItem(int id, [FromBody] UpdateItemDto updateItemDto, CancellationToken cancellationToken)
    {
        var item = await _itemService.UpdateItemAsync(id, updateItemDto, cancellationToken);
        return Ok(item);
    }

    /// <summary>
    /// Delete an item. (Requires Authentication)
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteItem(int id, CancellationToken cancellationToken)
    {
        await _itemService.DeleteItemAsync(id, cancellationToken);
        return NoContent();
    }
}
