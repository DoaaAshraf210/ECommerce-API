using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.Common.Models;
using ECommerce.Application.DTOs.Orders;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Domain.Entities;
using ECommerce.Application.Mappings;
using ECommerce.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _uow;

        public OrderService(IUnitOfWork uow) => _uow = uow;

        // ================= Checkout =================

        public async Task<OrderDto> CheckoutAsync(Guid userId, CheckoutDto dto)
        {
            var cart = await _uow.Carts.GetByUserIdWithItemsAsync(userId);
            if (cart is null || cart.Items.Count == 0)
                throw new BadRequestException("Your cart is empty.");

            var order = new Order
            {
                OrderNumber = GenerateOrderNumber(),
                UserId = userId,
                ShippingAddress = dto.ShippingAddress.ToEntity()
            };

            foreach (var item in cart.Items)
            {
                var product = item.Product;

                if (!product.IsActive)
                    throw new BadRequestException($"'{product.Name}' is no longer available.");

                if (item.Quantity > product.StockQuantity)
                    throw new ConflictException(
                        $"Only {product.StockQuantity} unit(s) of '{product.Name}' available.");

                product.StockQuantity -= item.Quantity;

                order.Items.Add(new OrderItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Sku = product.Sku,
                    UnitPrice = product.Price,
                    Quantity = item.Quantity
                });
            }

            order.TotalAmount = order.Items.Sum(i => i.LineTotal);

            await _uow.Orders.AddAsync(order);
            cart.Items.Clear();
            await _uow.SaveChangesAsync();   

            return order.ToDto();
        }

        // ================= client =================

        public async Task<PagedResult<OrderSummaryDto>> GetMyOrdersAsync(Guid userId, OrderQueryParams query)
            => await GetPagedInternalAsync(userId, query);

        public async Task<OrderDto> GetMyOrderAsync(Guid userId, int orderId)
        {
            var order = await GetOwnedOrderAsync(userId, orderId);
            return order.ToDto();
        }

        public async Task<OrderDto> CancelMyOrderAsync(Guid userId, int orderId)
        {
            var order = await GetOwnedOrderAsync(userId, orderId);

            if (order.Status != OrderStatus.Pending)
                throw new ConflictException("Only pending orders can be cancelled.");

            await CancelAndRestoreStockAsync(order);
            await _uow.SaveChangesAsync();

            return order.ToDto();
        }

        // ================= admin =================

        public async Task<PagedResult<OrderSummaryDto>> GetAllAsync(OrderQueryParams query)
            => await GetPagedInternalAsync(null, query);

        public async Task<OrderDto> GetByIdAsync(int orderId)
        {
            var order = await _uow.Orders.GetByIdWithItemsAsync(orderId)
                ?? throw new NotFoundException("Order", orderId);

            return order.ToDto();
        }

        public async Task<OrderDto> UpdateStatusAsync(int orderId, UpdateOrderStatusDto dto)
        {
            var order = await _uow.Orders.GetByIdWithItemsAsync(orderId)
                ?? throw new NotFoundException("Order", orderId);

            if (!order.CanTransitionTo(dto.Status))
                throw new ConflictException(
                    $"Cannot change order status from '{order.Status}' to '{dto.Status}'.");

            if (dto.Status == OrderStatus.Cancelled)
                await CancelAndRestoreStockAsync(order);
            else
                order.Status = dto.Status;

            await _uow.SaveChangesAsync();
            return order.ToDto();
        }

        // ================= helpers =================

        private async Task<PagedResult<OrderSummaryDto>> GetPagedInternalAsync(
            Guid? userId, OrderQueryParams q)
        {
            var pageNumber = Math.Max(1, q.PageNumber);
            var pageSize = Math.Clamp(q.PageSize, 1, 50);

            var result = await _uow.Orders.GetPagedAsync(pageNumber, pageSize, userId, q.Status);

            var items = result.Items.Select(o => o.ToSummaryDto()).ToList();
            return new PagedResult<OrderSummaryDto>(items, result.TotalCount, pageNumber, pageSize);
        }

        private async Task<Order> GetOwnedOrderAsync(Guid userId, int orderId)
        {
            var order = await _uow.Orders.GetByIdWithItemsAsync(orderId);

            if (order is null || order.UserId != userId)
                throw new NotFoundException("Order", orderId);

            return order;
        }

        private async Task CancelAndRestoreStockAsync(Order order)
        {
            var ids = order.Items.Select(i => i.ProductId).ToList();
            var products = await _uow.Products.GetByIdsAsync(ids);

            foreach (var item in order.Items)
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product is not null)
                    product.StockQuantity += item.Quantity;
            }

            order.Status = OrderStatus.Cancelled;
        }

        private static string GenerateOrderNumber()
            => $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
    }
}