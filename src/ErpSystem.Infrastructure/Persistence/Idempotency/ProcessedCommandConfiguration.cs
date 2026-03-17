using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ErpSystem.Infrastructure.Persistence.Idempotency;

public class ProcessedCommandConfiguration : IEntityTypeConfiguration<ProcessedCommand>
{
    public void Configure(EntityTypeBuilder<ProcessedCommand> builder)
    {
        builder.ToTable("ProcessedCommands");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CommandId)
            .IsRequired();

        builder.Property(x => x.ProcessedAt)
            .IsRequired();

        builder.HasIndex(x => x.CommandId)
            .IsUnique();
    }
}
