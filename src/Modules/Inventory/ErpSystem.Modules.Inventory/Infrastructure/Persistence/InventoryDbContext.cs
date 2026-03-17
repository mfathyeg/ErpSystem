using ErpSystem.Domain.Common.Services;
using ErpSystem.Modules.Inventory.Domain.Entities;
using ErpSystem.SharedKernel.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ErpSystem.Modules.Inventory.Infrastructure.Persistence;

public class InventoryDbContext : DbContext
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IPublisher _publisher;

    public DbSet<Product> Products => Set<Product>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    public InventoryDbContext(
        DbContextOptions<InventoryDbContext> options,
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

        modelBuilder.HasDefaultSchema("Inventory");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditableEntities();
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
