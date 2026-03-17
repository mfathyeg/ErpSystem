using ErpSystem.Domain.Common.Repositories;
using ErpSystem.Modules.Orders.Domain.Repositories;
using ErpSystem.SharedKernel.CQRS;
using ErpSystem.SharedKernel.Results;

namespace ErpSystem.Modules.Orders.Application.Commands.SubmitOrder;

public sealed record SubmitOrderCommand(Guid OrderId) : Command;

public sealed class SubmitOrderCommandHandler : ICommandHandler<SubmitOrderCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SubmitOrderCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(SubmitOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetWithItemsAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure(Error.NotFound);
        }

        order.Submit();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
