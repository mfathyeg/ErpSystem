using ErpSystem.Modules.Orders.Domain.Entities;
using ErpSystem.Modules.Orders.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ErpSystem.Modules.Orders.Infrastructure.Persistence;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(o => o.OrderNumber)
            .IsUnique();

        builder.Property(o => o.CustomerId)
            .IsRequired();

        builder.HasIndex(o => o.CustomerId);

        builder.Property(o => o.Status)
            .HasConversion(
                status => status.Id,
                id => OrderStatus.FromId(id)!)
            .HasColumnName("Status_Id");

        builder.Ignore(o => o.DomainEvents);

        // Store status name for querying
        builder.Property<string>("Status_Name")
            .HasMaxLength(50);

        builder.OwnsOne(o => o.ShippingAddress, address =>
        {
            address.Property(a => a.Street).HasColumnName("ShippingAddress_Street").HasMaxLength(200);
            address.Property(a => a.City).HasColumnName("ShippingAddress_City").HasMaxLength(100);
            address.Property(a => a.State).HasColumnName("ShippingAddress_State").HasMaxLength(100);
            address.Property(a => a.Country).HasColumnName("ShippingAddress_Country").HasMaxLength(100);
            address.Property(a => a.PostalCode).HasColumnName("ShippingAddress_PostalCode").HasMaxLength(20);
        });

        builder.OwnsOne(o => o.BillingAddress, address =>
        {
            address.Property(a => a.Street).HasColumnName("BillingAddress_Street").HasMaxLength(200);
            address.Property(a => a.City).HasColumnName("BillingAddress_City").HasMaxLength(100);
            address.Property(a => a.State).HasColumnName("BillingAddress_State").HasMaxLength(100);
            address.Property(a => a.Country).HasColumnName("BillingAddress_Country").HasMaxLength(100);
            address.Property(a => a.PostalCode).HasColumnName("BillingAddress_PostalCode").HasMaxLength(20);
        });

        builder.OwnsOne(o => o.SubTotal, money =>
        {
            money.Property(m => m.Amount).HasColumnName("SubTotal_Amount").HasPrecision(18, 4);
            money.Property(m => m.Currency).HasColumnName("SubTotal_Currency").HasMaxLength(3);
        });

        builder.OwnsOne(o => o.Tax, money =>
        {
            money.Property(m => m.Amount).HasColumnName("Tax_Amount").HasPrecision(18, 4);
            money.Property(m => m.Currency).HasColumnName("Tax_Currency").HasMaxLength(3);
        });

        builder.OwnsOne(o => o.ShippingCost, money =>
        {
            money.Property(m => m.Amount).HasColumnName("ShippingCost_Amount").HasPrecision(18, 4);
            money.Property(m => m.Currency).HasColumnName("ShippingCost_Currency").HasMaxLength(3);
        });

        builder.OwnsOne(o => o.Total, money =>
        {
            money.Property(m => m.Amount).HasColumnName("Total_Amount").HasPrecision(18, 4);
            money.Property(m => m.Currency).HasColumnName("Total_Currency").HasMaxLength(3);
        });

        builder.Property(o => o.Notes)
            .HasMaxLength(2000);

        builder.Property(o => o.CancellationReason)
            .HasMaxLength(500);

        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.ProductId)
            .IsRequired();

        builder.Property(i => i.ProductName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(i => i.Sku)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(i => i.Quantity)
            .IsRequired();

        builder.OwnsOne(i => i.UnitPrice, money =>
        {
            money.Property(m => m.Amount).HasColumnName("UnitPrice_Amount").HasPrecision(18, 4);
            money.Property(m => m.Currency).HasColumnName("UnitPrice_Currency").HasMaxLength(3);
        });

        builder.OwnsOne(i => i.LineTotal, money =>
        {
            money.Property(m => m.Amount).HasColumnName("LineTotal_Amount").HasPrecision(18, 4);
            money.Property(m => m.Currency).HasColumnName("LineTotal_Currency").HasMaxLength(3);
        });

        builder.HasIndex(i => i.OrderId);
    }
}
