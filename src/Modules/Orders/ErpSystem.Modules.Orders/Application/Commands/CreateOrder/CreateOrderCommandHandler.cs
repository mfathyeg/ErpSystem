using ErpSystem.Domain.Common.Repositories;
using ErpSystem.Domain.Common.ValueObjects;
using ErpSystem.Modules.Orders.Domain.Entities;
using ErpSystem.Modules.Orders.Domain.Repositories;
using ErpSystem.SharedKernel.CQRS;
using ErpSystem.SharedKernel.Results;

namespace ErpSystem.Modules.Orders.Application.Commands.CreateOrder;

public sealed class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Guid>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var shippingAddress = Address.Create(
            request.ShippingAddress.Street,
            request.ShippingAddress.City,
            request.ShippingAddress.State,
            request.ShippingAddress.Country,
            request.ShippingAddress.PostalCode);

        Address? billingAddress = null;
        if (request.BillingAddress is not null)
        {
            billingAddress = Address.Create(
                request.BillingAddress.Street,
                request.BillingAddress.City,
                request.BillingAddress.State,
                request.BillingAddress.Country,
                request.BillingAddress.PostalCode);
        }

        var order = Order.Create(
            request.CustomerId,
            shippingAddress,
            billingAddress,
            request.Currency);

        foreach (var item in request.Items)
        {
            var unitPrice = Money.Create(item.UnitPrice, request.Currency);
            order.AddItem(item.ProductId, item.ProductName, item.Sku, item.Quantity, unitPrice);
        }

        _orderRepository.Add(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(order.Id);
    }
}
