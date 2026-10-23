using ECommerce.Application.Common.Models;
using ECommerce.Application.DTOs.Orders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Application.Services
{
    public interface IOrderService
    {
        Task<OrderDto> CheckoutAsync(Guid userId, CheckoutDto dto);
        Task<PagedResult<OrderSummaryDto>> GetMyOrdersAsync(Guid userId, OrderQueryParams query);
        Task<OrderDto> GetMyOrderAsync(Guid userId, int orderId);
        Task<OrderDto> CancelMyOrderAsync(Guid userId, int orderId);

        Task<PagedResult<OrderSummaryDto>> GetAllAsync(OrderQueryParams query);
        Task<OrderDto> GetByIdAsync(int orderId);
        Task<OrderDto> UpdateStatusAsync(int orderId, UpdateOrderStatusDto dto);

    }
}
