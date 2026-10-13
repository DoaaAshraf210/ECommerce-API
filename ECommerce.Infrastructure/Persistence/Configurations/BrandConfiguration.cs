using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Infrastructure.Persistence.Configurations
{
    internal class BrandConfiguration : IEntityTypeConfiguration<Brand>
    {
        public void Configure(EntityTypeBuilder<Brand> builder)
        {
            builder.ToTable("Brands");

            builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
            builder.Property(p => p.LogoUrl).HasMaxLength(500);

            builder.HasIndex(p => p.Name).IsUnique().HasFilter("[IsDeleted] = 0");


            builder.HasQueryFilter(p => !p.IsDeleted);
        }
    }
}
