using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Infrastructure.Persistence.Configurations
{
    internal class RefreshTokenConfiguration:IEntityTypeConfiguration<RefreshToken>
    {


        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens");
            builder.Property(t => t.TokenHash).IsRequired().HasMaxLength(64);
            builder.Property(t => t.ReplacedByTokenHash).HasMaxLength(64);

            builder.HasIndex(t => t.TokenHash).IsUnique();


            builder.HasOne<ApplicationUser>()
                              .WithMany()
                              .HasForeignKey(t => t.UserId)
                              .OnDelete(DeleteBehavior.Cascade);










        }
    }
}
