using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Infrastructure.Persistence.Configurations
{
    internal class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");

            builder.Property(o => o.OrderNumber).IsRequired().HasMaxLength(30);
            builder.HasIndex(o => o.OrderNumber).IsUnique();

            builder.Property(o => o.TotalAmount).HasPrecision(18, 2);

            builder.OwnsOne(o => o.ShippingAddress, a =>
            {
                a.Property(x => x.FullName).IsRequired().HasMaxLength(100);
                a.Property(x => x.Phone).IsRequired().HasMaxLength(20);
                a.Property(x => x.Street).IsRequired().HasMaxLength(200);
                a.Property(x => x.City).IsRequired().HasMaxLength(100);
                a.Property(x => x.PostalCode).HasMaxLength(20);
            });

            builder.HasOne<ApplicationUser>()
                   .WithMany()
                   .HasForeignKey(o => o.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(o => o.UserId);

            builder.HasMany(o => o.Items)
                   .WithOne(i => i.Order)
                   .HasForeignKey(i => i.OrderId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
