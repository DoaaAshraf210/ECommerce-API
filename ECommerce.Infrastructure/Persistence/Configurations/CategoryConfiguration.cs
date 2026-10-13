using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Infrastructure.Persistence.Configurations
{
    internal class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("Categories");

            builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
            builder.Property(p => p.Description).HasMaxLength(1000);
            builder.Property(p => p.ImageUrl).HasMaxLength(500);
            

            builder.HasIndex(p => p.Name).IsUnique().HasFilter("[IsDeleted] = 0");



            builder.HasQueryFilter(p => !p.IsDeleted);
        }
    }
}
