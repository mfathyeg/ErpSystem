using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ErpSystem.Infrastructure.Persistence.Auditing;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.UserName)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.ActionType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.EntityName)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.EntityId)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.OldValues)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.NewValues)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.Timestamp)
            .IsRequired();

        builder.Property(x => x.IpAddress)
            .HasMaxLength(50);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.EntityName);
        builder.HasIndex(x => x.Timestamp);
    }
}
