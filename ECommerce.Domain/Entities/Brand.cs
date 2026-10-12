using ECommerce.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Domain.Entities
{
    public class Brand : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string? LogoUrl { get; set; }
        public List<Product> Products { get; set; } = new();
    }
}
