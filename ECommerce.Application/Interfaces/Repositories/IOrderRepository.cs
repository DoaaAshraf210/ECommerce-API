using ECommerce.Application.Common.Models;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Application.Interfaces.Repositories
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<Order?> GetByIdWithItemsAsync(int id);

        Task<PagedResult<Order>> GetPagedAsync(
            int pageNumber, int pageSize, Guid? userId, OrderStatus? status);
    }
}
