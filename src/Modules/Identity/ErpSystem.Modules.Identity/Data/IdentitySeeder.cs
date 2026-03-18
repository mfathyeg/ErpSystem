using ErpSystem.Modules.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace ErpSystem.Modules.Identity.Data;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        // Seed roles
        string[] roles = ["Admin", "Manager", "Employee", "Viewer"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid> { Name = role });
            }
        }

        // Seed admin user
        var adminEmail = "admin@duralux.sa";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = "admin",
                Email = adminEmail,
                FirstName = "أحمد",
                LastName = "محمد",
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // Seed manager user
        var managerEmail = "manager@duralux.sa";
        var managerUser = await userManager.FindByEmailAsync(managerEmail);

        if (managerUser == null)
        {
            managerUser = new ApplicationUser
            {
                UserName = "manager",
                Email = managerEmail,
                FirstName = "سارة",
                LastName = "علي",
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(managerUser, "Manager123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(managerUser, "Manager");
            }
        }

        // Seed employee user
        var employeeEmail = "user@duralux.sa";
        var employeeUser = await userManager.FindByEmailAsync(employeeEmail);

        if (employeeUser == null)
        {
            employeeUser = new ApplicationUser
            {
                UserName = "user",
                Email = employeeEmail,
                FirstName = "محمد",
                LastName = "خالد",
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(employeeUser, "User123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(employeeUser, "Employee");
            }
        }
    }
}
