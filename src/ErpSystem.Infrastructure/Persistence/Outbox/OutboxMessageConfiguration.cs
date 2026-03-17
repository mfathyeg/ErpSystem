using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ErpSystem.Infrastructure.Persistence.Outbox;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventType)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.Payload)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(x => x.OccurredOn)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Error)
            .HasMaxLength(2000);

        builder.HasIndex(x => new { x.Status, x.OccurredOn });
    }
}
