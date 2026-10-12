using ECommerce.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ECommerce.Domain.Entities
{
    public class ProductImage : BaseEntity
    {
        public string Url { get; set; } = null!;
        public bool IsMain { get; set; }
        public Product Product { get; set; } = new();
        public int ProductId { get; set; }
    }
}
