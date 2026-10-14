using ECommerce.Application.DTOs.Products;
using ECommerce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Application.Mappings
{
    public static class ProductMappings
    {
        public static ProductDto ToDto(this Product p) => new(
        p.Id,
        p.Name,
        p.Description,
        p.Sku,
        p.Price,
        p.StockQuantity,
        p.IsActive,
        p.CategoryId,
        p.Category.Name,
        p.BrandId,
        p.Brand.Name,
        p.Images.Select(i => i.Url).ToList());

        public static Product ToEntity(this CreateProductDto dto) => new()
        {
            Name = dto.Name,
            Description = dto.Description,
            Sku = dto.Sku,
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            IsActive = dto.IsActive,
            CategoryId = dto.CategoryId,
            BrandId = dto.BrandId
        };

        public static void UpdateFrom(this Product p, UpdateProductDto dto)
        {
            p.Name = dto.Name;
            p.Description = dto.Description;
            p.Sku = dto.Sku;
            p.Price = dto.Price;
            p.StockQuantity = dto.StockQuantity;
            p.IsActive = dto.IsActive;
            p.CategoryId = dto.CategoryId;
            p.BrandId = dto.BrandId;
        }
    }
}
