using ErpSystem.Domain.Common.Repositories;
using ErpSystem.Domain.Common.ValueObjects;
using ErpSystem.Modules.Inventory.Domain.Entities;
using ErpSystem.Modules.Inventory.Domain.Repositories;
using ErpSystem.Modules.Inventory.Domain.ValueObjects;
using ErpSystem.SharedKernel.CQRS;
using ErpSystem.SharedKernel.Results;

namespace ErpSystem.Modules.Inventory.Application.Commands.CreateProduct;

public sealed class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, Guid>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        if (await _productRepository.SkuExistsAsync(request.Sku, cancellationToken))
        {
            return Result.Failure<Guid>(Error.Conflict);
        }

        var category = ProductCategory.Create(request.CategoryCode, request.CategoryName);
        var unitPrice = Money.Create(request.UnitPrice, request.Currency);

        var product = Product.Create(
            request.Sku,
            request.Name,
            request.Description,
            category,
            unitPrice,
            request.InitialStock,
            request.ReorderLevel,
            request.SupplierId);

        _productRepository.Add(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(product.Id);
    }
}
