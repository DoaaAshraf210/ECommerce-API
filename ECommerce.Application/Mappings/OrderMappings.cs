using ECommerce.Application.DTOs.Orders;
using ECommerce.Domain.Entities;
using ECommerce.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Application.Mappings
{
    public static class OrderMappings
    {
        public static ShippingAddress ToEntity(this ShippingAddressDto dto) => new()
        {
            FullName = dto.FullName,
            Phone = dto.Phone,
            Street = dto.Street,
            City = dto.City,
            PostalCode = dto.PostalCode
        };

        public static ShippingAddressDto ToDto(this ShippingAddress a)
            => new(a.FullName, a.Phone, a.Street, a.City, a.PostalCode);

        public static OrderItemDto ToDto(this OrderItem i)
            => new(i.ProductId, i.ProductName, i.Sku, i.UnitPrice, i.Quantity, i.LineTotal);

        public static OrderDto ToDto(this Order o) => new(
            o.Id, o.OrderNumber, o.Status.ToString(), o.TotalAmount, o.CreatedAt,
            o.ShippingAddress.ToDto(),
            o.Items.Select(i => i.ToDto()).ToList());

        public static OrderSummaryDto ToSummaryDto(this Order o) => new(
            o.Id, o.OrderNumber, o.Status.ToString(), o.TotalAmount,
            o.Items.Sum(i => i.Quantity), o.CreatedAt);
    }
}