using ECommerce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using ECommerce.Application.DTOs.Categories;


namespace ECommerce.Application.Mappings
{
    public static class CategoryMapping
    {
        public static CategoryDto ToDto(this Category c) => new(
            c.Id,
            c.Name,
            c.Description,
            c.ImageUrl
        );

        public static Category ToEntity(this CreateCategoryDto dto) => new()
        {
            Name = dto.Name,
            Description = dto.Description,
            ImageUrl = dto.ImageUrl
        };

        public static void UpdateFrom(this Category c, UpdateCategoryDto dto)
        {
            c.Name = dto.Name;
            c.Description = dto.Description;
            c.ImageUrl = dto.ImageUrl;
        }
    }
}

   