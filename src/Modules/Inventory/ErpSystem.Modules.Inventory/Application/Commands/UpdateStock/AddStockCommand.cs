using ErpSystem.SharedKernel.CQRS;

namespace ErpSystem.Modules.Inventory.Application.Commands.UpdateStock;

public sealed record AddStockCommand(
    Guid ProductId,
    int Quantity,
    string Reason,
    Guid? ReferenceId = null) : Command;

public sealed record RemoveStockCommand(
    Guid ProductId,
    int Quantity,
    string Reason,
    Guid? ReferenceId = null) : Command;
