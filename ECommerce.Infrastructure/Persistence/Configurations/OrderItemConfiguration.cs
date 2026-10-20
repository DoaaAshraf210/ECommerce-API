using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Persistence.Configurations
{
    internal class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("OrderItems", t => t.HasCheckConstraint("CK_OrderItems_Quantity", "[Quantity] > 0"));
            builder.Property(o=>o.ProductName).IsRequired().HasMaxLength(200);
            builder.Property(o=>o.Sku).IsRequired().HasMaxLength(50);
            builder.Property(o => o.UnitPrice).HasPrecision(18, 2);


            builder.Ignore(i=>i.LineTotal);
            builder.HasIndex(i => new { i.OrderId, i.ProductId }).IsUnique();

            builder.HasOne<Product>().WithMany().HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Restrict);


        }
    }
}

