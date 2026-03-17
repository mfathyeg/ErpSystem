using ErpSystem.Modules.Inventory.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ErpSystem.Modules.Inventory.Application.EventHandlers;

public sealed class LowStockAlertHandler : INotificationHandler<LowStockAlertEvent>
{
    private readonly ILogger<LowStockAlertHandler> _logger;

    public LowStockAlertHandler(ILogger<LowStockAlertHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(LowStockAlertEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogWarning(
            "Low stock alert for product {Sku}: Current quantity {CurrentQuantity} is at or below reorder level {ReorderLevel}",
            notification.Sku,
            notification.CurrentQuantity,
            notification.ReorderLevel);

        // Here you would:
        // 1. Send notification to procurement team
        // 2. Create a reorder request
        // 3. Send email/push notification

        return Task.CompletedTask;
    }
}
