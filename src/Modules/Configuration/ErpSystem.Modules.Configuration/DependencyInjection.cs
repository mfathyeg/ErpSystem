using ErpSystem.Modules.Configuration.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ErpSystem.Modules.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddConfigurationModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ConfigurationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsHistoryTable("__EFMigrationsHistory", "Configuration")));

        return services;
    }
}
