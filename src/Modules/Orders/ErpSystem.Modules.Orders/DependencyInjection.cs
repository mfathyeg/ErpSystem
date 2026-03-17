using ErpSystem.Application;
using ErpSystem.Domain.Common.Repositories;
using ErpSystem.Modules.Orders.Domain.Repositories;
using ErpSystem.Modules.Orders.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ErpSystem.Modules.Orders;

public static class DependencyInjection
{
    public static IServiceCollection AddOrdersModule(this IServiceCollection services)
    {
        services.AddModuleApplication(typeof(DependencyInjection).Assembly);

        services.AddDbContext<OrdersDbContext>((sp, options) =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "Orders");
            });
        });

        services.AddScoped<IOrderRepository, OrderRepository>();

        return services;
    }
}
