using ErpSystem.Modules.Configuration.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ErpSystem.Modules.Configuration.Infrastructure.Persistence;

public class CompanySettingsConfiguration : IEntityTypeConfiguration<CompanySettings>
{
    public void Configure(EntityTypeBuilder<CompanySettings> builder)
    {
        builder.ToTable("CompanySettings");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.CompanyName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.Address)
            .HasMaxLength(500);

        builder.Property(c => c.Phone)
            .HasMaxLength(50);

        builder.Property(c => c.Email)
            .HasMaxLength(200);

        builder.Property(c => c.Website)
            .HasMaxLength(200);

        builder.Property(c => c.TaxId)
            .HasMaxLength(50);

        builder.Property(c => c.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(c => c.Timezone)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.DateFormat)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(c => c.LogoUrl)
            .HasMaxLength(500);

        builder.Ignore(c => c.DomainEvents);
    }
}

public class SystemConfigConfiguration : IEntityTypeConfiguration<SystemConfig>
{
    public void Configure(EntityTypeBuilder<SystemConfig> builder)
    {
        builder.ToTable("SystemConfigs");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Key)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(s => s.Key)
            .IsUnique();

        builder.Property(s => s.Value)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(s => s.Category)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(s => s.Description)
            .HasMaxLength(500);

        builder.Property(s => s.DataType)
            .HasMaxLength(20);

        builder.HasIndex(s => s.Category);

        builder.Ignore(s => s.DomainEvents);
    }
}

public class UserNotificationPrefsConfiguration : IEntityTypeConfiguration<UserNotificationPrefs>
{
    public void Configure(EntityTypeBuilder<UserNotificationPrefs> builder)
    {
        builder.ToTable("UserNotificationPrefs");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.UserId)
            .IsRequired();

        builder.HasIndex(u => u.UserId)
            .IsUnique();

        builder.Ignore(u => u.DomainEvents);
    }
}
