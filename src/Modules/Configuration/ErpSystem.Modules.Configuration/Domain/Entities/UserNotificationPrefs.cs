using ErpSystem.SharedKernel.Domain;

namespace ErpSystem.Modules.Configuration.Domain.Entities;

public class UserNotificationPrefs : AuditableAggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public bool EmailNotifications { get; private set; }
    public bool PushNotifications { get; private set; }
    public bool OrderUpdates { get; private set; }
    public bool InventoryAlerts { get; private set; }
    public bool SystemAlerts { get; private set; }

    private UserNotificationPrefs() { }

    public static UserNotificationPrefs Create(Guid userId)
    {
        return new UserNotificationPrefs
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EmailNotifications = true,
            PushNotifications = true,
            OrderUpdates = true,
            InventoryAlerts = true,
            SystemAlerts = true
        };
    }

    public void Update(
        bool emailNotifications,
        bool pushNotifications,
        bool orderUpdates,
        bool inventoryAlerts,
        bool systemAlerts)
    {
        EmailNotifications = emailNotifications;
        PushNotifications = pushNotifications;
        OrderUpdates = orderUpdates;
        InventoryAlerts = inventoryAlerts;
        SystemAlerts = systemAlerts;
    }
}
