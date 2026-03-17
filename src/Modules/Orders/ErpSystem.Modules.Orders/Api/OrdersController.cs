using Asp.Versioning;
using ErpSystem.Modules.Orders.Application.Commands.CreateOrder;
using ErpSystem.Modules.Orders.Application.Commands.SubmitOrder;
using ErpSystem.Modules.Orders.Application.Queries.GetOrder;
using ErpSystem.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ErpSystem.Modules.Orders.Api;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly ISender _sender;

    public OrdersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrder(Guid id)
    {
        var query = new GetOrderQuery(id);
        var result = await _sender.Send(query);
        return HandleResult(result);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var command = new CreateOrderCommand(
            request.CustomerId,
            new Application.Commands.CreateOrder.AddressDto(
                request.ShippingAddress.Street,
                request.ShippingAddress.City,
                request.ShippingAddress.State,
                request.ShippingAddress.Country,
                request.ShippingAddress.PostalCode),
            request.BillingAddress is not null
                ? new Application.Commands.CreateOrder.AddressDto(
                    request.BillingAddress.Street,
                    request.BillingAddress.City,
                    request.BillingAddress.State,
                    request.BillingAddress.Country,
                    request.BillingAddress.PostalCode)
                : null,
            request.Currency,
            request.Items.Select(i => new Application.Commands.CreateOrder.OrderItemDto(
                i.ProductId,
                i.ProductName,
                i.Sku,
                i.Quantity,
                i.UnitPrice)).ToList());

        var result = await _sender.Send(command);

        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetOrder), new { id = result.Value }, result.Value);
        }

        return BadRequest(new { result.Error.Code, result.Error.Message });
    }

    [HttpPost("{id:guid}/submit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubmitOrder(Guid id)
    {
        var command = new SubmitOrderCommand(id);
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

public record CreateOrderRequest(
    Guid CustomerId,
    AddressRequest ShippingAddress,
    AddressRequest? BillingAddress,
    string Currency,
    List<OrderItemRequest> Items);

public record AddressRequest(
    string Street,
    string City,
    string State,
    string Country,
    string PostalCode);

public record OrderItemRequest(
    Guid ProductId,
    string ProductName,
    string Sku,
    int Quantity,
    decimal UnitPrice);
