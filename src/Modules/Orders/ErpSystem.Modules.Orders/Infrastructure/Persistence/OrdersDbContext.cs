using ErpSystem.Domain.Common.Services;
using ErpSystem.Modules.Orders.Domain.Entities;
using ErpSystem.SharedKernel.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ErpSystem.Modules.Orders.Infrastructure.Persistence;

public class OrdersDbContext : DbContext
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IPublisher _publisher;

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public OrdersDbContext(
        DbContextOptions<OrdersDbContext> options,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IPublisher publisher) : base(options)
    {
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _publisher = publisher;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("Orders");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrdersDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditableEntities();
        UpdateOrderStatusNames();
        var domainEvents = GetDomainEvents();

        var result = await base.SaveChangesAsync(cancellationToken);

        await PublishDomainEvents(domainEvents, cancellationToken);

        return result;
    }

    private void UpdateAuditableEntities()
    {
        var entries = ChangeTracker.Entries<IAuditableEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = _dateTimeProvider.UtcNow;
                    entry.Entity.CreatedBy = _currentUserService.UserName;
                    break;

                case EntityState.Modified:
                    entry.Entity.LastModifiedAt = _dateTimeProvider.UtcNow;
                    entry.Entity.LastModifiedBy = _currentUserService.UserName;
                    break;
            }
        }
    }

    private void UpdateOrderStatusNames()
    {
        var orderEntries = ChangeTracker.Entries<Order>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in orderEntries)
        {
            entry.Property("Status_Name").CurrentValue = entry.Entity.Status.Name;
        }
    }

    private List<IDomainEvent> GetDomainEvents()
    {
        var domainEvents = ChangeTracker
            .Entries<Entity<Guid>>()
            .SelectMany(entry => entry.Entity.DomainEvents)
            .ToList();

        foreach (var entry in ChangeTracker.Entries<Entity<Guid>>())
        {
            entry.Entity.ClearDomainEvents();
        }

        return domainEvents;
    }

    private async Task PublishDomainEvents(List<IDomainEvent> domainEvents, CancellationToken cancellationToken)
    {
        foreach (var domainEvent in domainEvents)
        {
            await _publisher.Publish(domainEvent, cancellationToken);
        }
    }
}
