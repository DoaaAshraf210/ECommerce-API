using ECommerce.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ECommerce.Domain.Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string Sku { get; set; } = null!;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }

        public int BrandId { get; set; }
        public Brand Brand { get; set; } = null!;
        public int CategoryId { get; set; }
        public Category Category { get; set; } = new();
        public List<ProductImage> Images { get; set; } = new();
    }
}
