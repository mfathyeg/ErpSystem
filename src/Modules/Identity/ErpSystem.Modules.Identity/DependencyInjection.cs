using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ErpSystem.Modules.Identity;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Identity module configuration will be implemented with IdentityServer
        // This is a placeholder for the module registration

        return services;
    }
}
