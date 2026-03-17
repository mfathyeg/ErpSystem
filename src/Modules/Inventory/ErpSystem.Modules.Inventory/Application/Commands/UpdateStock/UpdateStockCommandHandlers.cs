using ErpSystem.Domain.Common.Repositories;
using ErpSystem.Modules.Inventory.Domain.Repositories;
using ErpSystem.SharedKernel.CQRS;
using ErpSystem.SharedKernel.Results;

namespace ErpSystem.Modules.Inventory.Application.Commands.UpdateStock;

public sealed class AddStockCommandHandler : ICommandHandler<AddStockCommand>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddStockCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AddStockCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Failure(Error.NotFound);
        }

        product.AddStock(request.Quantity, request.Reason, request.ReferenceId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public sealed class RemoveStockCommandHandler : ICommandHandler<RemoveStockCommand>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveStockCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RemoveStockCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Failure(Error.NotFound);
        }

        if (!product.StockQuantity.CanRemove(request.Quantity))
        {
            return Result.Failure(Error.Custom("Stock.Insufficient", "Insufficient stock available."));
        }

        product.RemoveStock(request.Quantity, request.Reason, request.ReferenceId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
