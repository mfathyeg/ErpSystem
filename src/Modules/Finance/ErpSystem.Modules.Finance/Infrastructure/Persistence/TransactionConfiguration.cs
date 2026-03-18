using ErpSystem.Modules.Finance.Domain.Entities;
using ErpSystem.Modules.Finance.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ErpSystem.Modules.Finance.Infrastructure.Persistence;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Reference)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(t => t.Reference)
            .IsUnique();

        builder.OwnsOne(t => t.Type, type =>
        {
            type.Property(tt => tt.Code)
                .HasColumnName("Type_Code")
                .HasMaxLength(20)
                .IsRequired();

            type.Property(tt => tt.Name)
                .HasColumnName("Type_Name")
                .HasMaxLength(50)
                .IsRequired();
        });

        builder.Property(t => t.Category)
            .HasMaxLength(100)
            .IsRequired();

        builder.OwnsOne(t => t.Amount, amount =>
        {
            amount.Property(m => m.Amount)
                .HasColumnName("Amount_Value")
                .HasPrecision(18, 4)
                .IsRequired();

            amount.Property(m => m.Currency)
                .HasColumnName("Amount_Currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.Property(t => t.Description)
            .HasMaxLength(500);

        builder.OwnsOne(t => t.Status, status =>
        {
            status.Property(ts => ts.Code)
                .HasColumnName("Status_Code")
                .HasMaxLength(20)
                .IsRequired();

            status.Property(ts => ts.Name)
                .HasColumnName("Status_Name")
                .HasMaxLength(50)
                .IsRequired();
        });

        builder.Property(t => t.TransactionDate)
            .IsRequired();

        builder.Property(t => t.RelatedEntityType)
            .HasMaxLength(100);

        builder.HasIndex(t => t.TransactionDate);
        builder.HasIndex(t => t.Category);

        builder.Ignore(t => t.DomainEvents);
    }
}
