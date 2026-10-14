using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Application.DTOs.Products
{
    public record ProductDto(
        int Id,
        string Name,
        string? Description,
        string Sku,
        decimal Price,
        int StockQuantity,
        bool IsActive,
        int CategoryId,
        string CategoryName,
        int BrandId,
        string BrandName,
        List<string> ImageUrls);

    public record CreateProductDto(
        string Name,
        string? Description,
        string Sku,
        decimal Price,
        int StockQuantity,
        bool IsActive,
        int CategoryId,
        int BrandId);

    public record UpdateProductDto(
        string Name,
        string? Description,
        string Sku,
        decimal Price,
        int StockQuantity,
        bool IsActive,
        int CategoryId,
        int BrandId);
    public class ProductQueryParams
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; }
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
    }
}
