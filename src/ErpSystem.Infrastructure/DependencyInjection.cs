using ErpSystem.Application.Abstractions.Auditing;
using ErpSystem.Application.Abstractions.Caching;
using ErpSystem.Application.Abstractions.Data;
using ErpSystem.Application.Abstractions.Idempotency;
using ErpSystem.Application.Abstractions.Messaging;
using ErpSystem.Application.Abstractions.Outbox;
using ErpSystem.Domain.Common.Repositories;
using ErpSystem.Domain.Common.Services;
using ErpSystem.Infrastructure.Data;
using ErpSystem.Infrastructure.Messaging;
using ErpSystem.Infrastructure.Persistence;
using ErpSystem.Infrastructure.Services;
using Hangfire;
using Hangfire.SqlServer;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;

namespace ErpSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddPersistence(configuration);
        services.AddCaching(configuration);
        services.AddMessaging(configuration);
        services.AddBackgroundJobs(configuration);
        services.AddInfrastructureServices();

        return services;
    }

    private static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ErpDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(ErpDbContext).Assembly.FullName);
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            }));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddSingleton<ISqlConnectionFactory>(_ => new SqlConnectionFactory(connectionString));

        return services;
    }

    private static IServiceCollection AddCaching(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetConnectionString("Redis");

        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "ErpSystem:";
            });
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        services.AddSingleton<ICacheService, CacheService>();

        return services;
    }

    private static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var rabbitMqSettings = configuration.GetSection("RabbitMQ");
        var useRabbitMq = rabbitMqSettings.GetValue<bool>("Enabled");

        if (useRabbitMq)
        {
            services.AddMassTransit(config =>
            {
                config.SetKebabCaseEndpointNameFormatter();

                config.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitMqSettings["Host"], rabbitMqSettings["VirtualHost"], h =>
                    {
                        h.Username(rabbitMqSettings["Username"]!);
                        h.Password(rabbitMqSettings["Password"]!);
                    });

                    cfg.ConfigureEndpoints(context);
                });
            });

            services.AddScoped<IEventBus, MassTransitEventBus>();
            services.AddHostedService<OutboxProcessor>();
        }
        else
        {
            services.AddMassTransit(config =>
            {
                config.UsingInMemory((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);
                });
            });
        }

        return services;
    }

    private static IServiceCollection AddBackgroundJobs(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddHangfire(config =>
        {
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180);
            config.UseSimpleAssemblyNameTypeSerializer();
            config.UseRecommendedSerializerSettings();
            config.UseSqlServerStorage(connectionString, new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true
            });
        });

        services.AddHangfireServer();

        return services;
    }

    private static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<IIdempotencyService, IdempotencyService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IOutboxService, OutboxService>();

        return services;
    }

    public static void ConfigureSerilog(
        IConfiguration configuration,
        LoggerConfiguration loggerConfiguration)
    {
        var seqUrl = configuration["Logging:Seq:ServerUrl"];
        var elasticUrl = configuration["Logging:Elasticsearch:NodeUrl"];

        loggerConfiguration
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "ErpSystem")
            .WriteTo.Console();

        if (!string.IsNullOrEmpty(seqUrl))
        {
            loggerConfiguration.WriteTo.Seq(seqUrl);
        }

        if (!string.IsNullOrEmpty(elasticUrl))
        {
            loggerConfiguration.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticUrl))
            {
                AutoRegisterTemplate = true,
                AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                IndexFormat = "erp-system-logs-{0:yyyy.MM.dd}"
            });
        }
    }
}
