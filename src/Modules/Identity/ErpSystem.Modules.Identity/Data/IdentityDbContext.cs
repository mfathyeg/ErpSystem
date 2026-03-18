using ErpSystem.Modules.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ErpSystem.Modules.Identity.Data;

public class IdentityDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema("identity");

        // Seed default roles
        builder.Entity<IdentityRole<Guid>>().HasData(
            new IdentityRole<Guid>
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Admin",
                NormalizedName = "ADMIN"
            },
            new IdentityRole<Guid>
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Manager",
                NormalizedName = "MANAGER"
            },
            new IdentityRole<Guid>
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "Employee",
                NormalizedName = "EMPLOYEE"
            },
            new IdentityRole<Guid>
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Name = "Viewer",
                NormalizedName = "VIEWER"
            }
        );
    }
}
