using ECommerce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Application.Mappings
{
    public static class BrandMapping
    {
        public static BrandDto ToDto(this Brand b) => new(
            b.Id,
            b.Name,
          b.LogoUrl
        );

        public static Brand ToEntity(this CreateBrandDto dto) => new()
        {
            Name = dto.Name,
            LogoUrl = dto.LogoUrl
        };

        public static void UpdateFrom(this Brand b, UpdateBrandDto dto)
        {
            b.Name = dto.Name;
            b.LogoUrl = dto.LogoUrl;
        }
    }
}



