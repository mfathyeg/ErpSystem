using ErpSystem.Application;
using ErpSystem.Domain.Common.Repositories;
using ErpSystem.Modules.Inventory.Domain.Repositories;
using ErpSystem.Modules.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ErpSystem.Modules.Inventory;

public static class DependencyInjection
{
    public static IServiceCollection AddInventoryModule(this IServiceCollection services)
    {
        services.AddModuleApplication(typeof(DependencyInjection).Assembly);

        services.AddDbContext<InventoryDbContext>((sp, options) =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "Inventory");
            });
        });

        services.AddScoped<IProductRepository, ProductRepository>();

        // Register InventoryDbContext as IUnitOfWork for this module
        services.AddScoped<IUnitOfWork>(sp =>
        {
            var context = sp.GetRequiredService<InventoryDbContext>();
            return new InventoryUnitOfWork(context);
        });

        return services;
    }
}

internal class InventoryUnitOfWork : IUnitOfWork
{
    private readonly InventoryDbContext _context;

    public InventoryUnitOfWork(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        var transaction = _context.Database.CurrentTransaction;
        if (transaction is not null)
        {
            await transaction.CommitAsync(cancellationToken);
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        var transaction = _context.Database.CurrentTransaction;
        if (transaction is not null)
        {
            await transaction.RollbackAsync(cancellationToken);
        }
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
