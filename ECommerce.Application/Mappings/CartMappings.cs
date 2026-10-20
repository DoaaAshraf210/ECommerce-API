using ECommerce.Application.DTOs.Carts;
using ECommerce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Application.Mappings
{
    public static class CartMappings
    {

        public static CartDto ToDto(this Cart cart)
        {
            var items = cart.Items
                .Select(i => new CartItemDto(
                    i.ProductId,
                    i.Product.Name,
                    i.Product.Sku,
                    i.Product.Images.OrderByDescending(img => img.IsMain)
                                    .Select(img => img.Url)
                                    .FirstOrDefault(),
                    i.Product.Price,
                    i.Quantity,
                    i.Product.Price * i.Quantity,
                    i.Product.StockQuantity))
                .ToList();

            return new CartDto(items, items.Sum(i => i.Quantity), items.Sum(i => i.LineTotal));
        }
    }
}