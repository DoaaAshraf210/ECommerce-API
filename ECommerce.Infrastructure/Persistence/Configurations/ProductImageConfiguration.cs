using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Infrastructure.Persistence.Configurations
{
    internal class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
    {
        public void Configure(EntityTypeBuilder<ProductImage> builder)
        {
            builder.ToTable("ProductImages");

            builder.Property(p => p.Url).IsRequired().HasMaxLength(500);
       

            builder.HasOne(p => p.Product)
                   .WithMany(p => p.Images)
                   .HasForeignKey(p => p.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);


            builder.HasQueryFilter(p => !p.IsDeleted);
        }
    }
}
