using ECommerce.Application.Common.Models;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Infrastructure.Repositories
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        public OrderRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Order?> GetByIdWithItemsAsync(int id)
            => await _set.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);

        public async Task<PagedResult<Order>> GetPagedAsync(
            int pageNumber, int pageSize, Guid? userId, OrderStatus? status)
        {
            var query = _set.AsNoTracking().Include(o => o.Items).AsQueryable();

            if (userId.HasValue)
                query = query.Where(o => o.UserId == userId.Value);

            if (status.HasValue)
                query = query.Where(o => o.Status == status.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(o => o.CreatedAt)
                .ThenByDescending(o => o.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Order>(items, totalCount, pageNumber, pageSize);

        }
    }
}