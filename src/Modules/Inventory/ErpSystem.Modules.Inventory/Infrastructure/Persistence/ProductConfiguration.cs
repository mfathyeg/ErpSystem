using ErpSystem.Modules.Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ErpSystem.Modules.Inventory.Infrastructure.Persistence;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Sku)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(p => p.Sku)
            .IsUnique();

        builder.Property(p => p.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(2000);

        builder.OwnsOne(p => p.Category, category =>
        {
            category.Property(c => c.Code)
                .HasColumnName("Category_Code")
                .HasMaxLength(20)
                .IsRequired();

            category.Property(c => c.Name)
                .HasColumnName("Category_Name")
                .HasMaxLength(100)
                .IsRequired();
        });

        builder.OwnsOne(p => p.UnitPrice, price =>
        {
            price.Property(m => m.Amount)
                .HasColumnName("UnitPrice_Amount")
                .HasPrecision(18, 4)
                .IsRequired();

            price.Property(m => m.Currency)
                .HasColumnName("UnitPrice_Currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.OwnsOne(p => p.StockQuantity, stock =>
        {
            stock.Property(s => s.Value)
                .HasColumnName("StockQuantity_Value")
                .IsRequired();
        });

        builder.Property(p => p.ReorderLevel)
            .IsRequired();

        builder.Property(p => p.IsActive)
            .IsRequired();

        builder.HasMany(p => p.StockMovements)
            .WithOne()
            .HasForeignKey(sm => sm.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(p => p.DomainEvents);
    }
}

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("StockMovements");

        builder.HasKey(sm => sm.Id);

        builder.Property(sm => sm.MovementType)
            .IsRequired();

        builder.Property(sm => sm.Quantity)
            .IsRequired();

        builder.Property(sm => sm.PreviousQuantity)
            .IsRequired();

        builder.Property(sm => sm.NewQuantity)
            .IsRequired();

        builder.Property(sm => sm.Reason)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(sm => sm.OccurredAt)
            .IsRequired();

        builder.HasIndex(sm => sm.ProductId);
        builder.HasIndex(sm => sm.OccurredAt);
    }
}
