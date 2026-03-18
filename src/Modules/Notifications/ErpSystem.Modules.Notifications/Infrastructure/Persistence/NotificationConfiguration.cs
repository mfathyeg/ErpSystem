using ErpSystem.Modules.Notifications.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ErpSystem.Modules.Notifications.Infrastructure.Persistence;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.UserId)
            .IsRequired();

        builder.Property(n => n.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(n => n.Message)
            .HasMaxLength(1000)
            .IsRequired();

        builder.OwnsOne(n => n.Type, type =>
        {
            type.Property(nt => nt.Code)
                .HasColumnName("Type_Code")
                .HasMaxLength(20)
                .IsRequired();

            type.Property(nt => nt.Name)
                .HasColumnName("Type_Name")
                .HasMaxLength(50)
                .IsRequired();
        });

        builder.Property(n => n.IsRead)
            .IsRequired();

        builder.Property(n => n.RelatedEntityType)
            .HasMaxLength(100);

        builder.HasIndex(n => n.UserId);
        builder.HasIndex(n => n.IsRead);
        builder.HasIndex(n => n.CreatedAt);

        builder.Ignore(n => n.DomainEvents);
    }
}
