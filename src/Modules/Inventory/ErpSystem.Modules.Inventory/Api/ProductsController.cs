using Asp.Versioning;
using ErpSystem.Modules.Inventory.Application.Commands.CreateProduct;
using ErpSystem.Modules.Inventory.Application.Commands.UpdateStock;
using ErpSystem.Modules.Inventory.Application.Queries.GetProduct;
using ErpSystem.Modules.Inventory.Application.Queries.GetProducts;
using ErpSystem.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ErpSystem.Modules.Inventory.Api;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly ISender _sender;

    public ProductsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts(
        [FromQuery] string? searchTerm,
        [FromQuery] string? categoryCode,
        [FromQuery] bool? isActive,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetProductsQuery(searchTerm, categoryCode, isActive, pageNumber, pageSize);
        var result = await _sender.Send(query);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProduct(Guid id)
    {
        var query = new GetProductQuery(id);
        var result = await _sender.Send(query);
        return HandleResult(result);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        var command = new CreateProductCommand(
            request.Sku,
            request.Name,
            request.Description,
            request.CategoryCode,
            request.CategoryName,
            request.UnitPrice,
            request.Currency,
            request.InitialStock,
            request.ReorderLevel,
            request.SupplierId);

        var result = await _sender.Send(command);

        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetProduct), new { id = result.Value }, result.Value);
        }

        return BadRequest(new { result.Error.Code, result.Error.Message });
    }

    [HttpPost("{id:guid}/add-stock")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddStock(Guid id, [FromBody] UpdateStockRequest request)
    {
        var command = new AddStockCommand(id, request.Quantity, request.Reason, request.ReferenceId);
        var result = await _sender.Send(command);
        return HandleResult(result);
    }

    [HttpPost("{id:guid}/remove-stock")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveStock(Guid id, [FromBody] UpdateStockRequest request)
    {
        var command = new RemoveStockCommand(id, request.Quantity, request.Reason, request.ReferenceId);
        var result = await _sender.Send(command);
        return HandleResult(result);
    }

    private IActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
        {
            return Ok();
        }

        return result.Error.Code switch
        {
            "Error.NotFound" => NotFound(new { result.Error.Code, result.Error.Message }),
            _ => BadRequest(new { result.Error.Code, result.Error.Message })
        };
    }

    private IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return result.Error.Code switch
        {
            "Error.NotFound" => NotFound(new { result.Error.Code, result.Error.Message }),
            _ => BadRequest(new { result.Error.Code, result.Error.Message })
        };
    }
}

public record CreateProductRequest(
    string Sku,
    string Name,
    string? Description,
    string CategoryCode,
    string CategoryName,
    decimal UnitPrice,
    string Currency,
    int InitialStock,
    int ReorderLevel,
    Guid? SupplierId);

public record UpdateStockRequest(
    int Quantity,
    string Reason,
    Guid? ReferenceId);
