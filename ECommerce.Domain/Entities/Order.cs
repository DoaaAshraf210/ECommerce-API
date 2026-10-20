using ECommerce.Domain.Common;
using ECommerce.Domain.Enums;
using ECommerce.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Domain.Entities
{
    public class Order : BaseEntity
    {
        public string OrderNumber { get; set; } = null!;
        public Guid UserId { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public decimal TotalAmount { get; set; }
        public ShippingAddress ShippingAddress { get; set; } = null!;
        public List<OrderItem> Items { get; set; } = new();
    }

  }