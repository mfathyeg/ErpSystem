using System.Reflection;
using ErpSystem.Application.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ErpSystem.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        services.AddAutoMapper(assembly);

        return services;
    }

    public static IServiceCollection AddModuleApplication(
        this IServiceCollection services,
        Assembly moduleAssembly)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(moduleAssembly);
        });

        services.AddValidatorsFromAssembly(moduleAssembly);
        services.AddAutoMapper(moduleAssembly);

        return services;
    }
}
