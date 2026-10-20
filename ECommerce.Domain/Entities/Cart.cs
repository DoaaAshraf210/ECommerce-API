using ECommerce.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Domain.Entities
{
    public class Cart:BaseEntity
    {
        public Guid UserId { get; set; }
        public List<CartItem> Items { get; set; } = new();
    }

    
}