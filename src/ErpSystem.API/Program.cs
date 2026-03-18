using Asp.Versioning;
using ErpSystem.API.Infrastructure;
using ErpSystem.API.Middleware;
using ErpSystem.Application;
using ErpSystem.Infrastructure;
using ErpSystem.Infrastructure.Persistence;
using ErpSystem.Modules.Configuration;
using ErpSystem.Modules.Finance;
using ErpSystem.Modules.Identity;
using ErpSystem.Modules.Inventory;
using ErpSystem.Modules.Notifications;
using ErpSystem.Modules.Orders;
using ErpSystem.Modules.Users;
using Hangfire;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Add modules
builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddUsersModule();
builder.Services.AddInventoryModule();
builder.Services.AddOrdersModule();
builder.Services.AddFinanceModule();
builder.Services.AddNotificationsModule();
builder.Services.AddConfigurationModule();

// Add API services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Api-Version"));
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "ERP System API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Authorization (Authentication is configured in Identity module)
builder.Services.AddAuthorization();

// SignalR
builder.Services.AddSignalR();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? Array.Empty<string>())
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Health Checks
builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")!, name: "sqlserver")
    .AddRedis(builder.Configuration.GetConnectionString("Redis") ?? "localhost", name: "redis");

// HTTP Context accessor for current user service
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ErpSystem.Domain.Common.Services.ICurrentUserService, CurrentUserService>();

var app = builder.Build();

// Middleware pipeline
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "ERP System API v1");
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAngular");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health checks
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// Hangfire dashboard
app.MapHangfireDashboard("/hangfire");

// SignalR hubs
app.MapHub<ErpSystem.Modules.Notifications.Hubs.NotificationHub>("/hubs/notifications");

// Apply migrations in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();

    // ERP DbContext migrations
    var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
    await dbContext.Database.MigrateAsync();

    // Identity DbContext migrations
    var identityDbContext = scope.ServiceProvider.GetRequiredService<ErpSystem.Modules.Identity.Data.IdentityDbContext>();
    await identityDbContext.Database.MigrateAsync();

    // Seed Identity data
    await ErpSystem.Modules.Identity.Data.IdentitySeeder.SeedAsync(app.Services);
}

app.Run();
